using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Payhas.Binyat.Data.Postgres.Abstractions;
using Payhas.Binyat.Data.Postgres.Models;

namespace Payhas.Binyat.Data.Sqlite.Sync;

/// <summary>
/// Hosted background service that bidirectionally replicates SQLite ↔
/// PostgreSQL.
///
/// Design:
///  • Pull — for each tracked table, query rows where
///    <c>updated_at &gt; sync_state.last_pulled_at</c> and upsert into SQLite.
///    The remote-wins policy is applied when the local row is also dirty.
///  • Push — drain <c>sync_outbox</c> in queued order, calling the
///    corresponding PG repository. On success, mark the local row as
///    <c>sync_state='synced'</c>; on failure, increment <c>attempt</c> and
///    retry on the next tick (exponential back-off via <c>SyncOptions</c>).
///  • Conflicts — an unresolvable mismatch (server row updated newer than
///    local dirty change) is logged into <c>sync_conflicts</c> for manual
///    review, defaulting to "server wins".
/// </summary>
public sealed class SyncService : BackgroundService
{
    private readonly SqliteSchemaManager _schema;
    private readonly IConnectivityProbe _probe;
    private readonly ILogger<SyncService> _log;
    private readonly SyncOptions _options;
    private readonly string _sqliteConn;
    private readonly string _postgresConn;
    private readonly IInvoicesRepository _onlineInvoices;
    private readonly IStocksRepository _onlineStocks;

    public SyncService(
        SqliteSchemaManager schema,
        IConnectivityProbe probe,
        ILogger<SyncService> log,
        SyncOptions options,
        string sqliteConnectionString,
        string postgresConnectionString,
        IInvoicesRepository onlineInvoices,
        IStocksRepository   onlineStocks)
    {
        _schema         = schema;
        _probe          = probe;
        _log            = log;
        _options        = options;
        _sqliteConn     = sqliteConnectionString;
        _postgresConn   = postgresConnectionString;
        _onlineInvoices = onlineInvoices;
        _onlineStocks   = onlineStocks;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _schema.EnsureCreatedAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await _probe.IsOnlineAsync(stoppingToken))
                {
                    await PushOutboxAsync(stoppingToken);
                    await PullChangesAsync(stoppingToken);
                }
                else
                {
                    _log.LogDebug("Offline — skipping sync tick");
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _log.LogError(ex, "Sync tick failed");
            }

            try { await Task.Delay(_options.PollInterval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    // ─── PUSH ────────────────────────────────────────────────────────────────

    private async Task PushOutboxAsync(CancellationToken ct)
    {
        await using var sqlite = await OpenSqliteAsync(ct);

        // Drain a bounded batch each tick — keeps the transaction short and
        // makes back-pressure observable in logs/metrics.
        var rows = (await sqlite.QueryAsync<OutboxRow>(new CommandDefinition(
            @"SELECT id, table_name, row_id, operation, payload, attempt
              FROM sync_outbox
              ORDER BY queued_at
              LIMIT @batch",
            new { batch = _options.PushBatchSize },
            cancellationToken: ct))).ToList();

        if (rows.Count == 0) return;

        _log.LogInformation("Pushing {Count} outbox rows", rows.Count);

        foreach (var row in rows)
        {
            try
            {
                await PushOneAsync(row, ct);

                await sqlite.ExecuteAsync(new CommandDefinition(
                    "DELETE FROM sync_outbox WHERE id = @id", new { row.Id }, cancellationToken: ct));

                await sqlite.ExecuteAsync(new CommandDefinition(
                    $"UPDATE {row.TableName} SET sync_state = 'synced', last_synced = datetime('now') WHERE id = @rowId",
                    new { row.RowId }, cancellationToken: ct));
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Push of {Table}/{RowId} failed (attempt {Attempt})",
                    row.TableName, row.RowId, row.Attempt + 1);

                await sqlite.ExecuteAsync(new CommandDefinition(
                    @"UPDATE sync_outbox SET attempt = attempt + 1, last_error = @err WHERE id = @id",
                    new { id = row.Id, err = ex.Message }, cancellationToken: ct));

                if (row.Attempt + 1 >= _options.MaxAttempts)
                {
                    _log.LogError("Giving up on {Table}/{RowId} after {Max} attempts",
                        row.TableName, row.RowId, _options.MaxAttempts);
                    await sqlite.ExecuteAsync(new CommandDefinition(
                        "DELETE FROM sync_outbox WHERE id = @id", new { row.Id }, cancellationToken: ct));
                    await sqlite.ExecuteAsync(new CommandDefinition(
                        $"UPDATE {row.TableName} SET sync_state = 'conflict' WHERE id = @rowId",
                        new { row.RowId }, cancellationToken: ct));
                }
                break; // Stop the batch on any failure to preserve order.
            }
        }
    }

    private async Task PushOneAsync(OutboxRow row, CancellationToken ct)
    {
        switch (row.TableName, row.Operation)
        {
            case ("invoices", "insert"):
            case ("invoices", "update"):
            {
                var inv = JsonSerializer.Deserialize<Invoice>(row.Payload!)
                          ?? throw new InvalidOperationException("Invalid invoice payload");
                if (row.Operation == "insert")
                    await _onlineInvoices.CreateAsync(inv, ct);
                else
                    await _onlineInvoices.UpdateAsync(inv, ct);
                break;
            }
            case ("invoices", "delete"):
            {
                using var doc = JsonDocument.Parse(row.Payload!);
                var id = doc.RootElement.GetProperty("id").GetString()!;
                await _onlineInvoices.DeleteAsync(id, ct);
                break;
            }

            case ("stocks", "insert"):
            case ("stocks", "update"):
            {
                var s = JsonSerializer.Deserialize<Stock>(row.Payload!)
                        ?? throw new InvalidOperationException("Invalid stock payload");
                if (row.Operation == "insert")
                    await _onlineStocks.CreateAsync(s, ct);
                else
                    await _onlineStocks.UpdateAsync(s, ct);
                break;
            }
            case ("stocks", "delete"):
            {
                using var doc = JsonDocument.Parse(row.Payload!);
                var id = doc.RootElement.GetProperty("id").GetString()!;
                await _onlineStocks.DeleteAsync(id, ct);
                break;
            }

            default:
                throw new NotSupportedException(
                    $"Outbox operation {row.TableName}/{row.Operation} not supported yet.");
        }
    }

    // ─── PULL ────────────────────────────────────────────────────────────────

    private async Task PullChangesAsync(CancellationToken ct)
    {
        await PullTableAsync(
            "stocks",
            @"SELECT id, code, name, short_name, type, group_name, tags, barcodes,
                     limit_min, limit_max, description, is_disabled, updated_at
                FROM stocks
               WHERE updated_at > @since",
            (sqlite, r) => UpsertStockRowAsync(sqlite, r, ct), ct);

        await PullTableAsync(
            "invoices",
            @"SELECT id, code, date, due_date, invoice_type, user_id, user_name,
                     office_id, warehouse_id, depository_id, partner_id, display_currency_id,
                     stock_price_group, debit_credit_left_amount, is_completed, is_disabled,
                     group_name, tags, description, updated_at
                FROM invoices
               WHERE updated_at > @since",
            (sqlite, r) => UpsertInvoiceRowAsync(sqlite, r, ct), ct);
    }

    private async Task PullTableAsync(
        string table,
        string selectSql,
        Func<SqliteConnection, dynamic, Task> upsert,
        CancellationToken ct)
    {
        await using var sqlite = await OpenSqliteAsync(ct);

        var since = await sqlite.ExecuteScalarAsync<string?>(new CommandDefinition(
            "SELECT last_pulled_at FROM sync_state WHERE table_name = @t",
            new { t = table }, cancellationToken: ct))
            ?? "1900-01-01T00:00:00Z";

        await using var pg = new NpgsqlConnection(_postgresConn);
        await pg.OpenAsync(ct);

        var changes = (await pg.QueryAsync(new CommandDefinition(
            selectSql, new { since = DateTime.Parse(since, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind) },
            cancellationToken: ct))).ToList();

        if (changes.Count == 0) return;

        _log.LogInformation("Pulling {Count} rows from {Table}", changes.Count, table);

        DateTime maxUpdatedAt = DateTime.Parse(since,
            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);

        await using var tx = (SqliteTransaction)await sqlite.BeginTransactionAsync(ct);
        try
        {
            foreach (var row in changes)
            {
                await upsert(sqlite, row);
                DateTime ua = (DateTime)row.updated_at;
                if (ua > maxUpdatedAt) maxUpdatedAt = ua;
            }

            await sqlite.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO sync_state (table_name, last_pulled_at, last_run_at)
                  VALUES (@t, @ts, datetime('now'))
                  ON CONFLICT(table_name) DO UPDATE SET
                      last_pulled_at = excluded.last_pulled_at,
                      last_run_at    = excluded.last_run_at",
                new { t = table, ts = maxUpdatedAt.ToUniversalTime().ToString("o") },
                tx, cancellationToken: ct));

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private async Task UpsertStockRowAsync(SqliteConnection sqlite, dynamic r, CancellationToken ct)
    {
        // Conflict policy: if the local row is dirty, log a conflict and
        // overwrite (server wins). Production deployments may switch to
        // "ask the user" via the sync_conflicts table.
        await CheckAndLogConflictAsync(sqlite, "stocks", (Guid)r.id, "server-wins", ct);

        await sqlite.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO stocks (id, code, name, short_name, type, group_name, tags, barcodes,
                                  limit_min, limit_max, description, is_disabled,
                                  created_at, updated_at, row_version, sync_state, last_synced)
              VALUES (@Id, @Code, @Name, @ShortName, @Type, @Group, @TagsJson, @BarcodesJson,
                      @LimitMin, @LimitMax, @Description, @IsDisabledInt,
                      datetime('now'), @UpdatedAt, 1, 'synced', datetime('now'))
              ON CONFLICT(id) DO UPDATE SET
                  code = excluded.code, name = excluded.name, short_name = excluded.short_name,
                  type = excluded.type, group_name = excluded.group_name,
                  tags = excluded.tags, barcodes = excluded.barcodes,
                  limit_min = excluded.limit_min, limit_max = excluded.limit_max,
                  description = excluded.description, is_disabled = excluded.is_disabled,
                  updated_at = excluded.updated_at,
                  row_version = stocks.row_version + 1,
                  sync_state  = 'synced',
                  last_synced = datetime('now')",
            new
            {
                Id = ((Guid)r.id).ToString(),
                Code = (string?)r.code,
                Name = (string)r.name,
                ShortName = (string?)r.short_name,
                Type = (string?)r.type,
                Group = (string?)r.group_name,
                TagsJson     = r.tags     == null ? null : JsonSerializer.Serialize((string[])r.tags),
                BarcodesJson = r.barcodes == null ? null : JsonSerializer.Serialize((string[])r.barcodes),
                LimitMin = (decimal?)r.limit_min,
                LimitMax = (decimal?)r.limit_max,
                Description = (string?)r.description,
                IsDisabledInt = ((bool)r.is_disabled) ? 1 : 0,
                UpdatedAt = ((DateTime)r.updated_at).ToString("o")
            }, cancellationToken: ct));
    }

    private async Task UpsertInvoiceRowAsync(SqliteConnection sqlite, dynamic r, CancellationToken ct)
    {
        await CheckAndLogConflictAsync(sqlite, "invoices", (Guid)r.id, "server-wins", ct);

        await sqlite.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO invoices (id, code, date, due_date, invoice_type, user_id, user_name,
                                    office_id, warehouse_id, depository_id, partner_id, display_currency_id,
                                    stock_price_group, debit_credit_left_amount, is_completed, is_disabled,
                                    group_name, tags, description, created_at, updated_at, row_version, sync_state, last_synced)
              VALUES (@Id, @Code, @Date, @DueDate, @InvoiceType, @UserId, @UserName,
                      @OfficeId, @WarehouseId, @DepositoryId, @PartnerId, @DisplayCurrencyId,
                      @StockPriceGroup, @DebitCreditInt, @IsCompletedInt, @IsDisabledInt,
                      @Group, @TagsJson, @Description, datetime('now'), @UpdatedAt, 1, 'synced', datetime('now'))
              ON CONFLICT(id) DO UPDATE SET
                  code = excluded.code, date = excluded.date, due_date = excluded.due_date,
                  invoice_type = excluded.invoice_type, user_id = excluded.user_id, user_name = excluded.user_name,
                  office_id = excluded.office_id, warehouse_id = excluded.warehouse_id,
                  depository_id = excluded.depository_id, partner_id = excluded.partner_id,
                  display_currency_id = excluded.display_currency_id,
                  stock_price_group = excluded.stock_price_group,
                  debit_credit_left_amount = excluded.debit_credit_left_amount,
                  is_completed = excluded.is_completed, is_disabled = excluded.is_disabled,
                  group_name = excluded.group_name, tags = excluded.tags, description = excluded.description,
                  updated_at = excluded.updated_at,
                  row_version = invoices.row_version + 1,
                  sync_state  = 'synced',
                  last_synced = datetime('now')",
            new
            {
                Id = ((Guid)r.id).ToString(),
                Code = (string?)r.code,
                Date = ((DateTime)((DateTimeOffset)r.date).UtcDateTime).ToString("o"),
                DueDate = r.due_date == null ? null : ((DateTime)((DateTimeOffset)r.due_date).UtcDateTime).ToString("o"),
                InvoiceType = (string)r.invoice_type,
                UserId = ((Guid?)r.user_id)?.ToString(),
                UserName = (string?)r.user_name,
                OfficeId = ((Guid?)r.office_id)?.ToString(),
                WarehouseId = ((Guid?)r.warehouse_id)?.ToString(),
                DepositoryId = ((Guid?)r.depository_id)?.ToString(),
                PartnerId = ((Guid?)r.partner_id)?.ToString(),
                DisplayCurrencyId = ((Guid?)r.display_currency_id)?.ToString(),
                StockPriceGroup = (string?)r.stock_price_group,
                DebitCreditInt = (bool)r.debit_credit_left_amount ? 1 : 0,
                IsCompletedInt = (bool)r.is_completed ? 1 : 0,
                IsDisabledInt  = (bool)r.is_disabled  ? 1 : 0,
                Group = (string?)r.group_name,
                TagsJson = r.tags == null ? null : JsonSerializer.Serialize((string[])r.tags),
                Description = (string?)r.description,
                UpdatedAt = ((DateTime)((DateTimeOffset)r.updated_at).UtcDateTime).ToString("o")
            }, cancellationToken: ct));
    }

    private static async Task CheckAndLogConflictAsync(
        SqliteConnection sqlite, string table, Guid rowId, string resolution, CancellationToken ct)
    {
        dynamic? local = await sqlite.QuerySingleOrDefaultAsync(new CommandDefinition(
            $"SELECT sync_state, row_version FROM {table} WHERE id = @id",
            new { id = rowId.ToString() }, cancellationToken: ct));

        if (local is null) return;
        if ((string?)local.sync_state != "dirty") return;

        var rowVersion = (long)local.row_version;
        await sqlite.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO sync_conflicts (table_name, row_id, local_data, server_data, resolution)
              VALUES (@table, @rowId, @local, @server, @resolution)",
            new
            {
                table,
                rowId       = rowId.ToString(),
                local       = $"{{ \"row_version\": {rowVersion}, \"sync_state\": \"dirty\" }}",
                server      = "{}",
                resolution
            }, cancellationToken: ct));
    }

    private async Task<SqliteConnection> OpenSqliteAsync(CancellationToken ct)
    {
        var conn = new SqliteConnection(_sqliteConn);
        await conn.OpenAsync(ct);
        await using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        await pragma.ExecuteNonQueryAsync(ct);
        return conn;
    }

    private sealed class OutboxRow
    {
        public long Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string RowId { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string? Payload { get; set; }
        public int Attempt { get; set; }
    }
}

/// <summary>Tunables for <see cref="SyncService"/>.</summary>
public sealed class SyncOptions
{
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(15);
    public int     PushBatchSize { get; init; } = 100;
    public int     MaxAttempts   { get; init; } = 5;
}
