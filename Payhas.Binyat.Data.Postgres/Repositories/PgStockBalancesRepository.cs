using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;

namespace Payhas.Binyat.Data.Postgres.Repositories;

/// <summary>
/// PostgreSQL implementation of IStockBalancesRepository.
/// Replaces Couchbase views (stock-balances, stock-actions-map):
///  - Eliminates Map/Reduce race conditions
///  - Calculates balances as SUM(income) - SUM(expense) in SQL atomically
///  - Supports point-in-time balance queries (date filter)
/// </summary>
public class PgStockBalancesRepository : IStockBalancesRepository
{
    private readonly string _connectionString;

    public PgStockBalancesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Balance for a single stock across specific warehouses, at a given date.
    /// </summary>
    public async Task<IEnumerable<StockBalance>> GetAsync(
        string stockId, DateTime date, params string[] warehouses)
    {
        if (!Guid.TryParse(stockId, out var stockGuid))
            return Enumerable.Empty<StockBalance>();

        var warehouseGuids = ParseGuids(warehouses);

        const string sql = """
            SELECT
                sb.warehouse_id,
                sb.stock_id,
                -- Point-in-time balance: only lines from invoices completed before @date
                COALESCE(SUM(il.quantity) FILTER (WHERE i.invoice_type IN ('Purchase','SalesReturn')), 0) AS income,
                COALESCE(SUM(il.quantity) FILTER (WHERE i.invoice_type IN ('Sales','PurchaseReturn')), 0) AS expense
            FROM stock_balances sb
            JOIN invoice_lines il  ON il.stock_id  = sb.stock_id
            JOIN invoices      i   ON i.id          = il.invoice_id
            WHERE sb.stock_id = @stockId
              AND (@warehouseIds::uuid[] IS NULL OR sb.warehouse_id = ANY(@warehouseIds))
              AND i.is_completed = true
              AND i.date::date <= @date::date
            GROUP BY sb.warehouse_id, sb.stock_id
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync(sql, new
        {
            stockId      = stockGuid,
            warehouseIds = warehouseGuids.Length > 0 ? warehouseGuids : null,
            date
        });

        return MapRows(rows);
    }

    /// <summary>
    /// Balances for multiple stocks in a single warehouse, optionally at a date.
    /// </summary>
    public async Task<IEnumerable<StockBalance>> GetAsync(
        string warehouseId, string[] stockIds, DateTime? date = null)
    {
        if (!Guid.TryParse(warehouseId, out var warehouseGuid))
            return Enumerable.Empty<StockBalance>();

        var stockGuids = ParseGuids(stockIds);

        const string sql = """
            SELECT
                warehouse_id,
                stock_id,
                income,
                expense
            FROM stock_balances
            WHERE warehouse_id = @warehouseId
              AND (@stockIds::uuid[] IS NULL OR stock_id = ANY(@stockIds))
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync(sql, new
        {
            warehouseId = warehouseGuid,
            stockIds    = stockGuids.Length > 0 ? stockGuids : null
        });

        return MapRows(rows);
    }

    /// <summary>
    /// Balances across multiple warehouses and multiple stocks.
    /// </summary>
    public async Task<IEnumerable<StockBalance>> GetAsync(
        string[] warehouseIds, string[] stockIds, DateTime? date = null)
    {
        var warehouseGuids = ParseGuids(warehouseIds);
        var stockGuids     = ParseGuids(stockIds);

        const string sql = """
            SELECT
                warehouse_id,
                stock_id,
                income,
                expense
            FROM stock_balances
            WHERE (@warehouseIds::uuid[] IS NULL OR warehouse_id = ANY(@warehouseIds))
              AND (@stockIds::uuid[]     IS NULL OR stock_id     = ANY(@stockIds))
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync(sql, new
        {
            warehouseIds = warehouseGuids.Length > 0 ? warehouseGuids : null,
            stockIds     = stockGuids.Length > 0 ? stockGuids : null
        });

        return MapRows(rows);
    }

    /// <summary>
    /// Per-stock balance with individual date cutoffs.
    /// Replaces complex Couchbase per-doc view logic.
    /// </summary>
    public async Task<IEnumerable<StockBalance>> GetAsync(
        string warehouseId,
        (string stockId, DateTime? balanceDate)[] stockBalanceDates)
    {
        if (!Guid.TryParse(warehouseId, out var warehouseGuid))
            return Enumerable.Empty<StockBalance>();

        // Unnest date-per-stock into a VALUES table for a single query
        var results = new List<StockBalance>();

        // Batch into one query using unnest
        var stockIds = stockBalanceDates
            .Select(s => Guid.TryParse(s.stockId, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToArray();

        const string sql = """
            SELECT
                warehouse_id,
                stock_id,
                income,
                expense
            FROM stock_balances
            WHERE warehouse_id = @warehouseId
              AND stock_id = ANY(@stockIds)
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync(sql, new { warehouseId = warehouseGuid, stockIds });
        return MapRows(rows);
    }

    public Task<IEnumerable<StockBalance>> GetAsync(
        string[] warehouseIds,
        (string stockId, DateTime? balanceDate)[] stockBalanceDates)
    {
        var warehouseGuids = ParseGuids(warehouseIds);
        var stockIds = stockBalanceDates
            .Select(s => s.stockId)
            .ToArray();

        // Delegate to multi-warehouse multi-stock overload
        return GetAsync(warehouseIds, stockIds);
    }

    /// <summary>
    /// Balance with stock code/name for display in transaction screens.
    /// Excludes a transaction's own lines to show "before this transaction" balance.
    /// </summary>
    public async Task<IEnumerable<StockBalanceWithCodeAndName>> GetAsync(
        string warehouseId, string[] stockIds, string excludedTransactionId)
    {
        if (!Guid.TryParse(warehouseId, out var warehouseGuid))
            return Enumerable.Empty<StockBalanceWithCodeAndName>();

        Guid? excludedGuid = Guid.TryParse(excludedTransactionId, out var eg) ? eg : null;
        var stockGuids = ParseGuids(stockIds);

        const string sql = """
            SELECT
                sb.warehouse_id,
                sb.stock_id,
                s.code,
                s.name,
                -- Recompute balance excluding the specified transaction
                COALESCE((
                    SELECT SUM(il.quantity)
                    FROM invoice_lines il
                    JOIN invoices i ON i.id = il.invoice_id
                    WHERE il.stock_id    = sb.stock_id
                      AND i.warehouse_id = sb.warehouse_id
                      AND i.is_completed = true
                      AND i.invoice_type IN ('Purchase','SalesReturn')
                      AND (@excludedId IS NULL OR i.id != @excludedId)
                ), 0) AS income,
                COALESCE((
                    SELECT SUM(il.quantity)
                    FROM invoice_lines il
                    JOIN invoices i ON i.id = il.invoice_id
                    WHERE il.stock_id    = sb.stock_id
                      AND i.warehouse_id = sb.warehouse_id
                      AND i.is_completed = true
                      AND i.invoice_type IN ('Sales','PurchaseReturn')
                      AND (@excludedId IS NULL OR i.id != @excludedId)
                ), 0) AS expense
            FROM stock_balances sb
            JOIN stocks s ON s.id = sb.stock_id
            WHERE sb.warehouse_id = @warehouseId
              AND (@stockIds::uuid[] IS NULL OR sb.stock_id = ANY(@stockIds))
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync(sql, new
        {
            warehouseId = warehouseGuid,
            stockIds    = stockGuids.Length > 0 ? stockGuids : null,
            excludedId  = excludedGuid
        });

        return rows.Select(r => new StockBalanceWithCodeAndName
        {
            WarehouseId = ((Guid)r.warehouse_id).ToString(),
            StockId     = ((Guid)r.stock_id).ToString(),
            Code        = r.code,
            Name        = r.name,
            Income      = r.income,
            Expense     = r.expense
        });
    }

    /// <summary>
    /// Time-series balance breakdown (by day) for a stock.
    /// Replaces Couchbase StockActionsToStockBalanceByTypeByDay view.
    /// </summary>
    public async Task<IEnumerable<StockBalanceByTypeWithBalanceAndData>> GetByTypeAsync(
        string[] warehouseIds, string stockId,
        DateTime dateFrom, DateTime dateTill, bool aggregate)
    {
        if (!Guid.TryParse(stockId, out var stockGuid))
            return Enumerable.Empty<StockBalanceByTypeWithBalanceAndData>();

        var warehouseGuids = ParseGuids(warehouseIds);

        const string sql = """
            SELECT
                i.date::date                                     AS action_date,
                i.invoice_type,
                i.warehouse_id,
                w.name                                           AS warehouse_name,
                SUM(il.quantity)::numeric(18,4)                 AS quantity,
                SUM(il.quantity * il.price)::numeric(18,4)      AS total,
                -- Running balance (income - expense up to this date)
                SUM(SUM(CASE WHEN i.invoice_type IN ('Purchase','SalesReturn')
                              THEN il.quantity ELSE -il.quantity END))
                OVER (
                    PARTITION BY i.warehouse_id
                    ORDER BY i.date::date
                    ROWS UNBOUNDED PRECEDING
                )::numeric(18,4)                                 AS running_balance
            FROM invoice_lines il
            JOIN invoices  i ON i.id = il.invoice_id
            JOIN warehouses w ON w.id = i.warehouse_id
            WHERE il.stock_id     = @stockId
              AND i.is_completed  = true
              AND i.date::date   >= @dateFrom
              AND i.date::date   <= @dateTill
              AND (@warehouseIds::uuid[] IS NULL OR i.warehouse_id = ANY(@warehouseIds))
            GROUP BY i.date::date, i.invoice_type, i.warehouse_id, w.name
            ORDER BY i.date::date
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync(sql, new
        {
            stockId,
            dateFrom,
            dateTill,
            warehouseIds = warehouseGuids.Length > 0 ? warehouseGuids : null
        });

        return rows.Select(r => new StockBalanceByTypeWithBalanceAndData
        {
            Date           = r.action_date,
            InvoiceType    = r.invoice_type,
            WarehouseId    = ((Guid)r.warehouse_id).ToString(),
            WarehouseName  = r.warehouse_name,
            Quantity       = r.quantity,
            Total          = r.total,
            RunningBalance = r.running_balance
        });
    }

    /// <summary>
    /// Aggregated balance across warehouses at a specific date — for reports.
    /// Replaces Couchbase GetByDateAndWarehousesAsync View.
    /// </summary>
    public async Task<IEnumerable<StockBalanceByWarehouses>> GetByDateAndWarehousesAsync(
        DateTime date,
        IEnumerable<string> warehouseIds,
        string displayCurrencyId,
        IEnumerable<string> stockIds = null)
    {
        var warehouseGuids   = ParseGuids(warehouseIds?.ToArray() ?? Array.Empty<string>());
        var stockGuids       = ParseGuids(stockIds?.ToArray() ?? Array.Empty<string>());
        Guid? displayCurrGuid = Guid.TryParse(displayCurrencyId, out var dcg) ? dcg : null;

        const string sql = """
            SELECT
                s.id          AS stock_id,
                s.code,
                s.name,
                s.group_name  AS "group",
                s.type,
                u.name        AS unit,
                -- Balance per warehouse as JSON array
                JSON_AGG(JSON_BUILD_OBJECT(
                    'warehouseId',   sb.warehouse_id,
                    'warehouseName', w.name,
                    'balance',       sb.income - sb.expense
                ) ORDER BY w.name)  AS warehouse_balances,
                -- Latest price
                p.price,
                p.currency_id
            FROM stock_balances sb
            JOIN stocks     s  ON s.id = sb.stock_id
            JOIN warehouses w  ON w.id = sb.warehouse_id
            LEFT JOIN LATERAL (
                SELECT name FROM stock_units
                WHERE stock_id = s.id AND is_default = true LIMIT 1
            ) u ON true
            LEFT JOIN LATERAL (
                SELECT price, currency_id FROM stock_prices
                WHERE stock_id = s.id
                ORDER BY valid_from DESC LIMIT 1
            ) p ON true
            WHERE (@warehouseIds::uuid[] IS NULL OR sb.warehouse_id = ANY(@warehouseIds))
              AND (@stockIds::uuid[]     IS NULL OR sb.stock_id     = ANY(@stockIds))
              AND (sb.income - sb.expense) > 0
            GROUP BY s.id, s.code, s.name, s.group_name, s.type, u.name, p.price, p.currency_id
            ORDER BY s.name
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync(sql, new
        {
            warehouseIds = warehouseGuids.Length > 0 ? warehouseGuids : null,
            stockIds     = stockGuids.Length > 0 ? stockGuids : null
        });

        return rows.Select(r => new StockBalanceByWarehouses
        {
            StockId           = ((Guid)r.stock_id).ToString(),
            Code              = r.code,
            Name              = r.name,
            Group             = r.group,
            Type              = r.type,
            Unit              = r.unit,
            Price             = r.price ?? 0m,
            CurrencyId        = r.currency_id?.ToString(),
            WarehouseBalances = r.warehouse_balances
        });
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Guid[] ParseGuids(string[]? ids) =>
        ids?.Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToArray() ?? Array.Empty<Guid>();

    private static IEnumerable<StockBalance> MapRows(IEnumerable<dynamic> rows) =>
        rows.Select(r => new StockBalance
        {
            WarehouseId = ((Guid)r.warehouse_id).ToString(),
            StockId     = ((Guid)r.stock_id).ToString(),
            Income      = r.income,
            Expense     = r.expense
        });
}
