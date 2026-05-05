using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Mermer.Data.Postgres.Abstractions;
using Mermer.Data.Postgres.Models;
using Mermer.Data.Postgres.Reports;

namespace Mermer.Data.Sqlite.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IInvoicesRepository"/> for the offline
/// cache. The financial-totals SQL is a 1:1 port of the PostgreSQL CTE in
/// <c>PgInvoicesRepository</c> (SQLite supports the same WITH/FILTER syntax
/// since 3.30, which Microsoft.Data.Sqlite ships).
/// </summary>
public sealed class SqliteInvoicesRepository : IInvoicesRepository
{
    private readonly string _connectionString;

    public SqliteInvoicesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Invoice?> GetAsync(string id, CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);
        var hdr = await conn.QuerySingleOrDefaultAsync(new CommandDefinition(
            "SELECT * FROM invoices WHERE id = @id", new { id }, cancellationToken: ct));
        if (hdr == null) return null;

        var inv = MapInvoice(hdr);
        await LoadChildrenAsync(conn, inv, ct);
        return inv;
    }

    public async Task<IReadOnlyList<InvoiceInfo>> GetInfoAsync(
        DateTime from, DateTime till,
        string? displayCurrencyId = null,
        CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);

        // Mirror of the PostgreSQL CTE — same currency-conversion model.
        // SQLite quirks: parameters can't use casts; we just rely on dynamic
        // typing and pass NULL or a TEXT GUID as @display.
        const string sql = @"
            WITH conv AS (
                SELECT invoice_id, currency_id, multiplier, divider
                FROM invoice_currency_convertions
            ),
            lines_agg AS (
                SELECT il.invoice_id,
                       COALESCE(SUM(
                           il.quantity * il.price
                           * COALESCE(c.multiplier * 1.0 / c.divider, 1)
                           * (CASE WHEN @display IS NOT NULL
                                   THEN COALESCE(d.divider * 1.0 / d.multiplier, 1)
                                   ELSE 1 END)
                       ), 0) AS subtotal
                FROM invoice_lines il
                LEFT JOIN conv c ON c.invoice_id = il.invoice_id AND c.currency_id = il.currency_id
                LEFT JOIN conv d ON d.invoice_id = il.invoice_id AND d.currency_id = @display
                GROUP BY il.invoice_id
            ),
            discounts_agg AS (
                SELECT id2.invoice_id,
                       COALESCE(SUM(
                           CASE id2.discount_type
                               WHEN 'Percentage' THEN COALESCE(la.subtotal,0) * id2.amount / 100
                               ELSE id2.amount
                                 * (CASE WHEN @display IS NOT NULL
                                         THEN COALESCE(d.divider * 1.0 / d.multiplier, 1)
                                         ELSE 1 END)
                           END), 0) AS discount_total
                FROM invoice_discounts id2
                LEFT JOIN lines_agg la ON la.invoice_id = id2.invoice_id
                LEFT JOIN conv d       ON d.invoice_id  = id2.invoice_id AND d.currency_id = @display
                GROUP BY id2.invoice_id
            ),
            payments_agg AS (
                SELECT ip.invoice_id,
                       COALESCE(SUM(CASE WHEN ip.payment_type = 'Payment' THEN
                           ip.amount
                           * COALESCE(c.multiplier * 1.0 / c.divider, 1)
                           * (CASE WHEN @display IS NOT NULL
                                   THEN COALESCE(d.divider * 1.0 / d.multiplier, 1)
                                   ELSE 1 END)
                         ELSE 0 END), 0) AS payment_total,
                       COALESCE(SUM(CASE WHEN ip.payment_type = 'Change'  THEN
                           ip.amount
                           * COALESCE(c.multiplier * 1.0 / c.divider, 1)
                           * (CASE WHEN @display IS NOT NULL
                                   THEN COALESCE(d.divider * 1.0 / d.multiplier, 1)
                                   ELSE 1 END)
                         ELSE 0 END), 0) AS change_total
                FROM invoice_payments ip
                LEFT JOIN conv c ON c.invoice_id = ip.invoice_id AND c.currency_id = ip.currency_id
                LEFT JOIN conv d ON d.invoice_id = ip.invoice_id AND d.currency_id = @display
                GROUP BY ip.invoice_id
            ),
            overheads_agg AS (
                SELECT io.invoice_id,
                       COALESCE(SUM(
                           io.amount
                           * COALESCE(c.multiplier * 1.0 / c.divider, 1)
                           * (CASE WHEN @display IS NOT NULL
                                   THEN COALESCE(d.divider * 1.0 / d.multiplier, 1)
                                   ELSE 1 END)
                       ), 0) AS overhead_total
                FROM invoice_overheads io
                LEFT JOIN conv c ON c.invoice_id = io.invoice_id AND c.currency_id = io.currency_id
                LEFT JOIN conv d ON d.invoice_id = io.invoice_id AND d.currency_id = @display
                GROUP BY io.invoice_id
            )
            SELECT
                i.id, i.code, i.date, i.invoice_type, i.is_completed,
                i.partner_id,    p.name AS partner_name,
                i.warehouse_id,  w.name AS warehouse_name,
                i.office_id,     o.name AS office_name,
                i.user_name,
                COALESCE(la.subtotal,       0) AS subtotal,
                COALESCE(da.discount_total, 0) AS discounts_total,
                COALESCE(oa.overhead_total, 0) AS overheads_total,
                (COALESCE(la.subtotal, 0)
                 - COALESCE(da.discount_total, 0)
                 + COALESCE(oa.overhead_total, 0)) AS grand_total,
                COALESCE(pa.payment_total, 0) AS payments_total,
                MAX(0,
                    COALESCE(la.subtotal, 0)
                    - COALESCE(da.discount_total, 0)
                    + COALESCE(oa.overhead_total, 0)
                    - COALESCE(pa.payment_total, 0)
                    + COALESCE(pa.change_total,  0)) AS left_total
            FROM invoices i
            LEFT JOIN partners      p  ON p.id  = i.partner_id
            LEFT JOIN warehouses    w  ON w.id  = i.warehouse_id
            LEFT JOIN offices       o  ON o.id  = i.office_id
            LEFT JOIN lines_agg     la ON la.invoice_id = i.id
            LEFT JOIN discounts_agg da ON da.invoice_id = i.id
            LEFT JOIN payments_agg  pa ON pa.invoice_id = i.id
            LEFT JOIN overheads_agg oa ON oa.invoice_id = i.id
            WHERE i.date >= @from AND i.date < @till
              AND i.is_disabled = 0
            ORDER BY i.date DESC";

        var rows = await conn.QueryAsync(new CommandDefinition(sql,
            new
            {
                from = from.ToString("o"),
                till = till.AddDays(1).ToString("o"),
                display = displayCurrencyId
            },
            cancellationToken: ct));

        return rows.Select(r => new InvoiceInfo
        {
            Id            = (string)r.id,
            Code          = (string?)r.code,
            Date          = ParseDate(r.date),
            InvoiceType   = Enum.Parse<InvoiceType>((string)r.invoice_type),
            IsCompleted   = ((long)r.is_completed) != 0,
            PartnerId     = (string?)r.partner_id,
            PartnerName   = (string?)r.partner_name,
            WarehouseId   = (string?)r.warehouse_id,
            WarehouseName = (string?)r.warehouse_name,
            OfficeId      = (string?)r.office_id,
            OfficeName    = (string?)r.office_name,
            UserName      = (string?)r.user_name,
            Subtotal      = ToDecimal(r.subtotal),
            DiscountsTotal = ToDecimal(r.discounts_total),
            OverheadsTotal = ToDecimal(r.overheads_total),
            GrandTotal    = ToDecimal(r.grand_total),
            PaymentsTotal = ToDecimal(r.payments_total),
            LeftTotal     = ToDecimal(r.left_total)
        }).ToList();
    }

    public async Task<int> CountInfoAsync(DateTime from, DateTime till, CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);
        return await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            @"SELECT COUNT(*) FROM invoices
              WHERE date >= @from AND date < @till AND is_disabled = 0",
            new { from = from.ToString("o"), till = till.AddDays(1).ToString("o") },
            cancellationToken: ct));
    }

    public Task<IReadOnlyList<InvoicePaymentInfo>> GetPaymentInfoAsync(
        DateTime from, DateTime till, string? officeId, string? partnerId,
        string? displayCurrencyId = null,
        CancellationToken ct = default)
    {
        // Partner debit/credit ledger isn't replicated locally yet — the
        // offline client only sees its own activity. The online repository
        // (PostgreSQL) is the source of truth for partner_actions.
        IReadOnlyList<InvoicePaymentInfo> empty = Array.Empty<InvoicePaymentInfo>();
        return Task.FromResult(empty);
    }

    public Task<int> CountPaymentInfoAsync(
        DateTime from, DateTime till, string? officeId, string? partnerId, CancellationToken ct = default)
        => Task.FromResult(0);

    public async Task<Invoice> CreateAsync(Invoice model, CancellationToken ct = default)
    {
        model.Id ??= Guid.NewGuid().ToString();
        await using var conn = await OpenAsync(ct);
        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(InsertHeaderSql, ToHeaderParams(model, dirty: true), tx, cancellationToken: ct));
            await ReplaceChildrenAsync(conn, tx, model, ct);
            await EnqueueOutboxAsync(conn, tx, "invoices", model.Id!, "insert", model, ct);
            await tx.CommitAsync(ct);
            return model;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Invoice> UpdateAsync(Invoice model, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(model.Id))
            throw new ArgumentException("Invoice ID required", nameof(model));

        await using var conn = await OpenAsync(ct);
        await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(UpdateHeaderSql, ToHeaderParams(model, dirty: true), tx, cancellationToken: ct));
            await ReplaceChildrenAsync(conn, tx, model, ct);
            await EnqueueOutboxAsync(conn, tx, "invoices", model.Id!, "update", model, ct);
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
                @"UPDATE invoices SET is_disabled = 1, updated_at = datetime('now'),
                                      row_version = row_version + 1, sync_state = 'dirty'
                  WHERE id = @id",
                new { id }, tx, cancellationToken: ct));
            await EnqueueOutboxAsync(conn, tx, "invoices", id, "delete", new { id }, ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<RevenueReportRow>> GetRevenueReportAsync(
        DateTime from, DateTime till, string? warehouseId = null, CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);

        // Pull the full history up to `till`; the running weighted-average
        // calculator needs everything before the window too.
        var baseSql = @"
            SELECT
                i.date          AS Date,
                i.id            AS InvoiceId,
                i.code          AS InvoiceCode,
                i.invoice_type  AS InvoiceType,
                il.id           AS LineId,
                il.source_id    AS SourceLineId,
                il.stock_id     AS StockId,
                s.code          AS StockCode,
                s.name          AS StockName,
                i.warehouse_id  AS WarehouseId,
                w.name          AS WarehouseName,
                il.quantity     AS Quantity,
                il.price        AS UnitPrice
            FROM invoices i
            JOIN invoice_lines il ON il.invoice_id = i.id
            LEFT JOIN stocks     s ON s.id = il.stock_id
            LEFT JOIN warehouses w ON w.id = i.warehouse_id
            WHERE i.is_completed = 1
              AND i.is_disabled  = 0
              AND i.date <= @till";
        if (warehouseId != null)
            baseSql += " AND i.warehouse_id = @warehouseId";
        baseSql += " ORDER BY i.date ASC, il.id ASC";

        var raw = await conn.QueryAsync(new CommandDefinition(baseSql, new
        {
            till = till.AddDays(1).ToString("o"),
            warehouseId
        }, cancellationToken: ct));

        var movements = raw.Select(r => new RevenueCalculator.StockMovement
        {
            Date          = ParseDate(r.Date),
            InvoiceId     = (string)r.InvoiceId,
            InvoiceCode   = (string?)r.InvoiceCode,
            InvoiceType   = Enum.Parse<InvoiceType>((string)r.InvoiceType),
            LineId        = (string)r.LineId,
            SourceLineId  = (string?)r.SourceLineId,
            StockId       = (string?)r.StockId,
            StockCode     = (string?)r.StockCode,
            StockName     = (string?)r.StockName,
            WarehouseId   = (string?)r.WarehouseId,
            WarehouseName = (string?)r.WarehouseName,
            Quantity      = ToDecimal(r.Quantity),
            UnitPrice     = ToDecimal(r.UnitPrice)
        });

        return RevenueCalculator.Build(movements, from, till);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<SqliteConnection> OpenAsync(CancellationToken ct)
    {
        var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        await pragma.ExecuteNonQueryAsync(ct);
        return conn;
    }

    private const string InsertHeaderSql = @"
        INSERT INTO invoices (id, code, date, due_date, invoice_type, user_id, user_name,
                              office_id, warehouse_id, depository_id, partner_id, display_currency_id,
                              stock_price_group, debit_credit_left_amount, is_completed, is_disabled,
                              group_name, tags, description, created_at, updated_at, row_version, sync_state)
        VALUES (@Id, @Code, @Date, @DueDate, @InvoiceType, @UserId, @UserName,
                @OfficeId, @WarehouseId, @DepositoryId, @PartnerId, @DisplayCurrencyId,
                @StockPriceGroup, @DebitCreditInt, @IsCompletedInt, @IsDisabledInt,
                @Group, @TagsJson, @Description, datetime('now'), datetime('now'), 1, @SyncState)";

    private const string UpdateHeaderSql = @"
        UPDATE invoices SET
            code = @Code, date = @Date, due_date = @DueDate, invoice_type = @InvoiceType,
            user_id = @UserId, user_name = @UserName, office_id = @OfficeId, warehouse_id = @WarehouseId,
            depository_id = @DepositoryId, partner_id = @PartnerId, display_currency_id = @DisplayCurrencyId,
            stock_price_group = @StockPriceGroup, debit_credit_left_amount = @DebitCreditInt,
            is_completed = @IsCompletedInt, is_disabled = @IsDisabledInt,
            group_name = @Group, tags = @TagsJson, description = @Description,
            updated_at = datetime('now'), row_version = row_version + 1, sync_state = @SyncState
        WHERE id = @Id";

    private static object ToHeaderParams(Invoice m, bool dirty) => new
    {
        m.Id, m.Code,
        Date            = m.Date.ToString("o"),
        DueDate         = m.DueDate?.ToString("o"),
        InvoiceType     = m.InvoiceType.ToString(),
        m.UserId, m.UserName,
        m.OfficeId, m.WarehouseId, m.DepositoryId, m.PartnerId, m.DisplayCurrencyId,
        m.StockPriceGroup,
        DebitCreditInt = m.DebitCreditLeftAmount ? 1 : 0,
        IsCompletedInt = m.IsCompleted           ? 1 : 0,
        IsDisabledInt  = m.IsDisabled            ? 1 : 0,
        m.Group,
        TagsJson       = m.Tags != null ? JsonSerializer.Serialize(m.Tags) : null,
        m.Description,
        SyncState      = dirty ? "dirty" : "synced"
    };

    private static async Task ReplaceChildrenAsync(
        SqliteConnection conn, SqliteTransaction tx, Invoice m, CancellationToken ct)
    {
        // Wipe & re-insert is the simplest correct approach for invoice
        // children. The number of rows is tiny per invoice (lines/discounts/
        // payments/overheads typically < 100 each), so this is cheap.
        var p = new { id = m.Id };
        foreach (var table in new[]
        {
            "invoice_lines", "invoice_discounts", "invoice_payments", "invoice_overheads"
        })
        {
            await conn.ExecuteAsync(new CommandDefinition(
                $"DELETE FROM {table} WHERE invoice_id = @id", p, tx, cancellationToken: ct));
        }

        foreach (var l in m.Lines)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO invoice_lines (id, invoice_id, source_id, stock_id, unit_id,
                                             quantity, price, currency_id, sort_order)
                  VALUES (@Id, @InvoiceId, @SourceId, @StockId, @UnitId,
                          @Quantity, @Price, @CurrencyId, @SortOrder)",
                new { Id = l.Id ?? Guid.NewGuid().ToString(), InvoiceId = m.Id, l.SourceId, l.StockId, l.UnitId,
                      l.Quantity, l.Price, l.CurrencyId, l.SortOrder },
                tx, cancellationToken: ct));
        }

        foreach (var d in m.Discounts)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO invoice_discounts (id, invoice_id, discount_type, amount, description, sort_order)
                  VALUES (@Id, @InvoiceId, @Type, @Amount, @Description, @SortOrder)",
                new { Id = d.Id ?? Guid.NewGuid().ToString(), InvoiceId = m.Id,
                      Type = d.Type.ToString(), d.Amount, d.Description, d.SortOrder },
                tx, cancellationToken: ct));
        }

        foreach (var (p2, type) in m.Payments.Select(x => (x, "Payment"))
                                  .Concat(m.Changes.Select(x => (x, "Change"))))
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO invoice_payments (id, invoice_id, payment_type, amount, currency_id, sort_order)
                  VALUES (@Id, @InvoiceId, @Type, @Amount, @CurrencyId, @SortOrder)",
                new { Id = p2.Id ?? Guid.NewGuid().ToString(), InvoiceId = m.Id, Type = type, p2.Amount, p2.CurrencyId, p2.SortOrder },
                tx, cancellationToken: ct));
        }

        foreach (var o in m.Overheads)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO invoice_overheads (id, invoice_id, amount, currency_id, description, sort_order)
                  VALUES (@Id, @InvoiceId, @Amount, @CurrencyId, @Description, @SortOrder)",
                new { Id = o.Id ?? Guid.NewGuid().ToString(), InvoiceId = m.Id, o.Amount, o.CurrencyId, o.Description, o.SortOrder },
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

    private static async Task LoadChildrenAsync(SqliteConnection conn, Invoice inv, CancellationToken ct)
    {
        inv.Lines = (await conn.QueryAsync(new CommandDefinition(
            "SELECT * FROM invoice_lines WHERE invoice_id = @id ORDER BY sort_order",
            new { id = inv.Id }, cancellationToken: ct)))
            .Select(l => new InvoiceLine
            {
                Id         = (string)l.id,
                SourceId   = (string?)l.source_id,
                StockId    = (string?)l.stock_id,
                UnitId     = (string?)l.unit_id,
                Quantity   = ToDecimal(l.quantity),
                Price      = ToDecimal(l.price),
                CurrencyId = (string?)l.currency_id,
                SortOrder  = (int)(long)l.sort_order
            }).ToList();

        inv.Discounts = (await conn.QueryAsync(new CommandDefinition(
            "SELECT * FROM invoice_discounts WHERE invoice_id = @id ORDER BY sort_order",
            new { id = inv.Id }, cancellationToken: ct)))
            .Select(d => new InvoiceDiscount
            {
                Id          = (string)d.id,
                Type        = (string)d.discount_type == "Percentage" ? InvoiceDiscountType.Percentage : InvoiceDiscountType.Flat,
                Amount      = ToDecimal(d.amount),
                Description = (string?)d.description,
                SortOrder   = (int)(long)d.sort_order
            }).ToList();

        var paymentRows = (await conn.QueryAsync(new CommandDefinition(
            "SELECT * FROM invoice_payments WHERE invoice_id = @id ORDER BY sort_order",
            new { id = inv.Id }, cancellationToken: ct))).ToList();
        inv.Payments = paymentRows.Where(p => (string)p.payment_type == "Payment").Select(MapPayment).ToList();
        inv.Changes  = paymentRows.Where(p => (string)p.payment_type == "Change").Select(MapPayment).ToList();

        inv.Overheads = (await conn.QueryAsync(new CommandDefinition(
            "SELECT * FROM invoice_overheads WHERE invoice_id = @id ORDER BY sort_order",
            new { id = inv.Id }, cancellationToken: ct)))
            .Select(o => new InvoiceOverhead
            {
                Id          = (string)o.id,
                Amount      = ToDecimal(o.amount),
                CurrencyId  = (string?)o.currency_id,
                Description = (string?)o.description,
                SortOrder   = (int)(long)o.sort_order
            }).ToList();
    }

    private static InvoicePayment MapPayment(dynamic p) => new()
    {
        Id         = (string)p.id,
        Amount     = ToDecimal(p.amount),
        CurrencyId = (string?)p.currency_id,
        SortOrder  = (int)(long)p.sort_order
    };

    private static Invoice MapInvoice(dynamic r) => new()
    {
        Id                    = (string)r.id,
        Code                  = (string?)r.code,
        Date                  = ParseDate(r.date),
        DueDate               = r.due_date == null ? null : ParseDate(r.due_date),
        InvoiceType           = Enum.Parse<InvoiceType>((string)r.invoice_type),
        UserId                = (string?)r.user_id,
        UserName              = (string?)r.user_name,
        OfficeId              = (string?)r.office_id,
        WarehouseId           = (string?)r.warehouse_id,
        DepositoryId          = (string?)r.depository_id,
        PartnerId             = (string?)r.partner_id,
        DisplayCurrencyId     = (string?)r.display_currency_id,
        StockPriceGroup       = (string?)r.stock_price_group,
        DebitCreditLeftAmount = ((long)r.debit_credit_left_amount) != 0,
        IsCompleted           = ((long)r.is_completed) != 0,
        IsDisabled            = ((long)r.is_disabled) != 0,
        Group                 = (string?)r.group_name,
        Tags                  = ParseJsonArray((string?)r.tags),
        Description           = (string?)r.description
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

    private static DateTime ParseDate(object v) => v switch
    {
        DateTime dt => dt,
        string s    => DateTime.Parse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind),
        _           => DateTime.UtcNow
    };
}
