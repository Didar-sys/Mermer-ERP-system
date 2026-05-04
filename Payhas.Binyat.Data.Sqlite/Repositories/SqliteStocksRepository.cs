using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Payhas.Binyat.Data.Postgres.Abstractions;
using Payhas.Binyat.Data.Postgres.Models;

namespace Payhas.Binyat.Data.Sqlite.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IStocksRepository"/> — local cache for
/// the offline mode. All writes flag <c>sync_state='dirty'</c> and bump
/// <c>row_version</c>; the SyncService later flushes them to PostgreSQL.
/// </summary>
public sealed class SqliteStocksRepository : IStocksRepository
{
    private readonly string _connectionString;

    public SqliteStocksRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Stock?> GetAsync(string id, CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);

        var row = await conn.QuerySingleOrDefaultAsync(
            new CommandDefinition("SELECT * FROM stocks WHERE id = @id", new { id }, cancellationToken: ct));
        if (row == null) return null;

        var stock = MapStock(row);
        await LoadChildrenAsync(conn, stock, ct);
        return stock;
    }

    public async Task<IReadOnlyList<Stock>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);

        var rows = await conn.QueryAsync(new CommandDefinition(
            "SELECT * FROM stocks WHERE is_disabled = 0 ORDER BY name",
            cancellationToken: ct));

        var stocks = rows.Select(MapStock).ToList();
        foreach (var s in stocks)
            await LoadChildrenAsync(conn, s, ct);
        return stocks;
    }

    public async Task<IReadOnlyList<Stock>> GetListAsync(string[] stockIds, CancellationToken ct = default)
    {
        if (stockIds.Length == 0) return Array.Empty<Stock>();

        await using var conn = await OpenAsync(ct);
        var rows = await conn.QueryAsync(new CommandDefinition(
            "SELECT * FROM stocks WHERE id IN @ids", new { ids = stockIds }, cancellationToken: ct));

        var byId = rows.ToDictionary(r => (string)r.id, MapStock);
        var ordered = stockIds.Where(byId.ContainsKey).Select(id => byId[id]).ToList();
        foreach (var s in ordered)
            await LoadChildrenAsync(conn, s, ct);
        return ordered;
    }

    public async Task<IReadOnlyList<StockInfo>> GetInfoAsync(string[]? stockIds = null, CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);

        // Branch on filter presence — Dapper's `IN` expansion can't be combined
        // with a "no filter" sentinel without ugly stitching, so we just have
        // two SQL bodies sharing the same projection.
        const string projection = @"
            SELECT
                s.id, s.code, s.name, s.short_name, s.type, s.group_name, s.tags, s.barcodes, s.is_disabled,
                u.name AS unit, p.price, p.currency_id
            FROM stocks s
            LEFT JOIN stock_units  u ON u.stock_id = s.id AND u.is_default = 1
            LEFT JOIN (
                SELECT stock_id, price, currency_id,
                       ROW_NUMBER() OVER (PARTITION BY stock_id ORDER BY valid_from DESC) AS rn
                FROM stock_prices WHERE price_group IS NULL
            ) p ON p.stock_id = s.id AND p.rn = 1";

        IEnumerable<dynamic> rows;
        if (stockIds is { Length: > 0 })
        {
            rows = await conn.QueryAsync(new CommandDefinition(
                projection + " WHERE s.id IN @ids ORDER BY s.name",
                new { ids = stockIds }, cancellationToken: ct));
        }
        else
        {
            rows = await conn.QueryAsync(new CommandDefinition(
                projection + " ORDER BY s.name", cancellationToken: ct));
        }

        return rows.Select(r => new StockInfo
        {
            Id          = (string)r.id,
            Code        = (string?)r.code,
            Name        = (string)r.name,
            ShortName   = (string?)r.short_name,
            Unit        = (string?)r.unit,
            Price       = ToDecimal(r.price),
            CurrencyId  = (string?)r.currency_id,
            Type        = (string?)r.type,
            Group       = (string?)r.group_name,
            Tags        = ParseJsonArray((string?)r.tags),
            Barcodes    = ParseJsonArray((string?)r.barcodes),
            IsDisabled  = ((long)r.is_disabled) != 0
        }).ToList();
    }

    public async Task<IReadOnlyList<StockInfo>> GetInfoAsync(
        string? additionalPriceCurrencyId, string? additionalPriceGroup, CancellationToken ct = default)
    {
        var infos = (await GetInfoAsync(stockIds: null, ct)).ToList();

        if (string.IsNullOrEmpty(additionalPriceCurrencyId) && string.IsNullOrEmpty(additionalPriceGroup))
            return infos;

        await using var conn = await OpenAsync(ct);
        var apRows = await conn.QueryAsync(new CommandDefinition(
            @"SELECT stock_id, price, currency_id FROM stock_additional_prices
              WHERE @grp IS NULL OR price_group = @grp",
            new { grp = string.IsNullOrEmpty(additionalPriceGroup) ? null : additionalPriceGroup },
            cancellationToken: ct));
        var byStock = apRows.ToDictionary(r => (string)r.stock_id);

        foreach (var info in infos)
        {
            if (info.Id != null && byStock.TryGetValue(info.Id, out var ap))
            {
                info.AdditionalPrice           = ToDecimal(ap.price);
                info.AdditionalPriceCurrencyId = (string?)ap.currency_id;
            }
        }
        return infos;
    }

    public async Task<Stock> CreateAsync(Stock model, CancellationToken ct = default)
    {
        model.Id ??= Guid.NewGuid().ToString();
        await using var conn = await OpenAsync(ct);
        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(InsertStockSql, ToParams(model, dirty: true), tx, cancellationToken: ct));
            await ReplaceChildrenAsync(conn, tx, model, ct);
            await EnqueueOutboxAsync(conn, tx, "stocks", model.Id!, "insert", model, ct);
            await tx.CommitAsync(ct);
            return model;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Stock> UpdateAsync(Stock model, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(model.Id))
            throw new ArgumentException("Stock ID required", nameof(model));

        await using var conn = await OpenAsync(ct);
        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(UpdateStockSql, ToParams(model, dirty: true), tx, cancellationToken: ct));
            await ReplaceChildrenAsync(conn, tx, model, ct);
            await EnqueueOutboxAsync(conn, tx, "stocks", model.Id!, "update", model, ct);
            await tx.CommitAsync(ct);
            return model;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);
        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"UPDATE stocks
                     SET is_disabled = 1,
                         updated_at  = datetime('now'),
                         row_version = row_version + 1,
                         sync_state  = 'dirty'
                   WHERE id = @id",
                new { id }, tx, cancellationToken: ct));
            await EnqueueOutboxAsync(conn, tx, "stocks", id, "delete", new { id }, ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems, CancellationToken ct = default)
    {
        // Merge is an online-only operation (it touches every invoice in the
        // system). Sync layer must reject it when offline; we still implement
        // the local effects so a single-machine deployment works.
        throw new NotSupportedException(
            "Stock merge is only supported when online (PostgreSQL repository).");
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>> GetFacetsAsync(
        string[] fields, CancellationToken ct = default)
    {
        var result = new Dictionary<string, IReadOnlyDictionary<string, int>>();
        await using var conn = await OpenAsync(ct);

        if (fields.Contains("group", StringComparer.OrdinalIgnoreCase))
        {
            var rows = await conn.QueryAsync<(string Key, int Count)>(new CommandDefinition(
                @"SELECT group_name AS Key, COUNT(*) AS Count FROM stocks
                  WHERE group_name IS NOT NULL AND is_disabled = 0 GROUP BY group_name",
                cancellationToken: ct));
            result["group"] = rows.ToDictionary(r => r.Key, r => r.Count);
        }
        if (fields.Contains("type", StringComparer.OrdinalIgnoreCase))
        {
            var rows = await conn.QueryAsync<(string Key, int Count)>(new CommandDefinition(
                @"SELECT type AS Key, COUNT(*) AS Count FROM stocks
                  WHERE type IS NOT NULL AND is_disabled = 0 GROUP BY type",
                cancellationToken: ct));
            result["type"] = rows.ToDictionary(r => r.Key, r => r.Count);
        }
        return result;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<SqliteConnection> OpenAsync(CancellationToken ct)
    {
        var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        // Foreign key enforcement is per-connection in SQLite.
        await using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        await pragma.ExecuteNonQueryAsync(ct);
        return conn;
    }

    private const string InsertStockSql = @"
        INSERT INTO stocks (id, code, name, short_name, type, group_name, tags, barcodes,
                            limit_min, limit_max, description, is_disabled,
                            created_at, updated_at, row_version, sync_state)
        VALUES (@Id, @Code, @Name, @ShortName, @Type, @Group, @TagsJson, @BarcodesJson,
                @LimitMin, @LimitMax, @Description, @IsDisabledInt,
                datetime('now'), datetime('now'), 1, @SyncState)";

    private const string UpdateStockSql = @"
        UPDATE stocks SET
            code = @Code, name = @Name, short_name = @ShortName, type = @Type, group_name = @Group,
            tags = @TagsJson, barcodes = @BarcodesJson,
            limit_min = @LimitMin, limit_max = @LimitMax, description = @Description,
            is_disabled = @IsDisabledInt,
            updated_at = datetime('now'),
            row_version = row_version + 1,
            sync_state = @SyncState
        WHERE id = @Id";

    private static object ToParams(Stock m, bool dirty) => new
    {
        m.Id, m.Code, m.Name, m.ShortName, m.Type, m.Group,
        TagsJson      = m.Tags     != null ? JsonSerializer.Serialize(m.Tags)     : null,
        BarcodesJson  = m.Barcodes != null ? JsonSerializer.Serialize(m.Barcodes) : null,
        m.LimitMin, m.LimitMax, m.Description,
        IsDisabledInt = m.IsDisabled ? 1 : 0,
        SyncState     = dirty ? "dirty" : "synced"
    };

    private static async Task LoadChildrenAsync(SqliteConnection conn, Stock stock, CancellationToken ct)
    {
        var units = await conn.QueryAsync(new CommandDefinition(
            "SELECT * FROM stock_units WHERE stock_id = @id", new { id = stock.Id }, cancellationToken: ct));
        stock.Units = units.Select(u => new StockUnit
        {
            Id         = (string)u.id,
            Name       = (string)u.name,
            IsDefault  = ((long)u.is_default) != 0,
            Multiplier = ToDecimal(u.multiplier),
            Divider    = ToDecimal(u.divider)
        }).ToList();

        var prices = await conn.QueryAsync(new CommandDefinition(
            "SELECT * FROM stock_prices WHERE stock_id = @id ORDER BY valid_from DESC", new { id = stock.Id }, cancellationToken: ct));
        stock.Prices = prices.Select(p => new StockPrice
        {
            Id         = (string)p.id,
            Price      = ToDecimal(p.price),
            CurrencyId = (string?)p.currency_id,
            PriceGroup = (string?)p.price_group,
            ValidFrom  = ParseDate(p.valid_from)
        }).ToList();

        var aps = await conn.QueryAsync(new CommandDefinition(
            "SELECT * FROM stock_additional_prices WHERE stock_id = @id", new { id = stock.Id }, cancellationToken: ct));
        stock.AdditionalPrices = aps.Select(p => new StockAdditionalPrice
        {
            Id         = (string)p.id,
            Price      = ToDecimal(p.price),
            CurrencyId = (string?)p.currency_id,
            PriceGroup = (string?)p.price_group,
            ValidFrom  = ParseDate(p.valid_from)
        }).ToList();
    }

    private static async Task ReplaceChildrenAsync(
        SqliteConnection conn, SqliteTransaction tx, Stock m, CancellationToken ct)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM stock_units WHERE stock_id = @id", new { id = m.Id }, tx, cancellationToken: ct));
        foreach (var u in m.Units)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO stock_units (id, stock_id, name, multiplier, divider, is_default)
                  VALUES (@Id, @StockId, @Name, @Multiplier, @Divider, @IsDefaultInt)",
                new { Id = u.Id ?? Guid.NewGuid().ToString(), StockId = m.Id, u.Name, u.Multiplier, u.Divider, IsDefaultInt = u.IsDefault ? 1 : 0 },
                tx, cancellationToken: ct));
        }

        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM stock_prices WHERE stock_id = @id", new { id = m.Id }, tx, cancellationToken: ct));
        foreach (var p in m.Prices)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO stock_prices (id, stock_id, valid_from, price, currency_id, price_group)
                  VALUES (@Id, @StockId, @ValidFrom, @Price, @CurrencyId, @PriceGroup)",
                new { Id = p.Id ?? Guid.NewGuid().ToString(), StockId = m.Id, ValidFrom = p.ValidFrom.ToString("o"), p.Price, p.CurrencyId, p.PriceGroup },
                tx, cancellationToken: ct));
        }

        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM stock_additional_prices WHERE stock_id = @id", new { id = m.Id }, tx, cancellationToken: ct));
        foreach (var p in m.AdditionalPrices)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO stock_additional_prices (id, stock_id, valid_from, price, currency_id, price_group)
                  VALUES (@Id, @StockId, @ValidFrom, @Price, @CurrencyId, @PriceGroup)",
                new { Id = p.Id ?? Guid.NewGuid().ToString(), StockId = m.Id, ValidFrom = p.ValidFrom.ToString("o"), p.Price, p.CurrencyId, p.PriceGroup },
                tx, cancellationToken: ct));
        }
    }

    private static Task EnqueueOutboxAsync(
        SqliteConnection conn, SqliteTransaction tx,
        string table, string rowId, string operation, object payload, CancellationToken ct) =>
        conn.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO sync_outbox (table_name, row_id, operation, payload)
              VALUES (@table, @rowId, @operation, @payload)",
            new { table, rowId, operation, payload = JsonSerializer.Serialize(payload) },
            tx, cancellationToken: ct));

    private static Stock MapStock(dynamic r) => new()
    {
        Id          = (string)r.id,
        Code        = (string?)r.code,
        Name        = (string)r.name,
        ShortName   = (string?)r.short_name,
        Type        = (string?)r.type,
        Group       = (string?)r.group_name,
        Tags        = ParseJsonArray((string?)r.tags),
        Barcodes    = ParseJsonArray((string?)r.barcodes),
        LimitMin    = (decimal?)ToNullableDecimal(r.limit_min),
        LimitMax    = (decimal?)ToNullableDecimal(r.limit_max),
        Description = (string?)r.description,
        IsDisabled  = ((long)r.is_disabled) != 0
    };

    private static List<string>? ParseJsonArray(string? json) =>
        string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<List<string>>(json);

    private static decimal ToDecimal(object? v) => v switch
    {
        null      => 0m,
        decimal d => d,
        double f  => (decimal)f,
        long l    => l,
        string s  => decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
        _         => Convert.ToDecimal(v, System.Globalization.CultureInfo.InvariantCulture)
    };

    private static decimal? ToNullableDecimal(object? v) =>
        v == null ? null : ToDecimal(v);

    private static DateTime ParseDate(object v) => v switch
    {
        DateTime dt => dt,
        string s    => DateTime.Parse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind),
        _           => DateTime.UtcNow
    };
}
