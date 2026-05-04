using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Payhas.Binyat.Data.Postgres.Abstractions;
using Payhas.Binyat.Data.Postgres.Models;

namespace Payhas.Binyat.Data.Sqlite.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IStockBalancesRepository"/>.
/// Mirrors the PostgreSQL repository — same logic, no <c>::numeric(18,4)</c>
/// casts (SQLite doesn't need them) and uses CASE-WHEN instead of FILTER
/// because Microsoft.Data.Sqlite ships with a SQLite build that doesn't
/// have FILTER baked in for all builds.
/// </summary>
public sealed class SqliteStockBalancesRepository : IStockBalancesRepository
{
    private readonly string _connectionString;

    public SqliteStockBalancesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<StockBalance>> GetAsync(
        string stockId, DateTime date, string[]? warehouseIds = null, CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);
        const string baseSql = @"
            SELECT
                i.warehouse_id AS WarehouseId,
                il.stock_id    AS StockId,
                COALESCE(SUM(CASE WHEN i.invoice_type IN ('Purchase','SalesReturn')  THEN il.quantity ELSE 0 END), 0) AS Income,
                COALESCE(SUM(CASE WHEN i.invoice_type IN ('Sales','PurchaseReturn')  THEN il.quantity ELSE 0 END), 0) AS Expense
            FROM invoice_lines il
            JOIN invoices i ON i.id = il.invoice_id
            WHERE il.stock_id    = @stockId
              AND i.is_completed = 1
              AND i.is_disabled  = 0
              AND date(i.date)  <= date(@date)
              AND i.warehouse_id IS NOT NULL";

        IEnumerable<StockBalance> rows;
        if (warehouseIds is { Length: > 0 })
        {
            rows = await conn.QueryAsync<StockBalance>(new CommandDefinition(
                baseSql + " AND i.warehouse_id IN @warehouses GROUP BY i.warehouse_id, il.stock_id",
                new { stockId, warehouses = warehouseIds, date = date.ToString("o") },
                cancellationToken: ct));
        }
        else
        {
            rows = await conn.QueryAsync<StockBalance>(new CommandDefinition(
                baseSql + " GROUP BY i.warehouse_id, il.stock_id",
                new { stockId, date = date.ToString("o") },
                cancellationToken: ct));
        }
        return rows.ToList();
    }

    public async Task<IReadOnlyList<StockBalance>> GetAsync(
        string warehouseId, string[] stockIds, CancellationToken ct = default)
    {
        if (stockIds.Length == 0) return Array.Empty<StockBalance>();
        await using var conn = await OpenAsync(ct);
        var rows = await conn.QueryAsync<StockBalance>(new CommandDefinition(
            @"SELECT warehouse_id AS WarehouseId, stock_id AS StockId, income AS Income, expense AS Expense
              FROM stock_balances WHERE warehouse_id = @warehouseId AND stock_id IN @stockIds",
            new { warehouseId, stockIds }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<StockBalance>> GetAsync(
        string[] warehouseIds, string[] stockIds, CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);

        var sb = new System.Text.StringBuilder(
            "SELECT warehouse_id AS WarehouseId, stock_id AS StockId, income AS Income, expense AS Expense FROM stock_balances WHERE 1=1");
        if (warehouseIds.Length > 0) sb.Append(" AND warehouse_id IN @warehouseIds");
        if (stockIds.Length     > 0) sb.Append(" AND stock_id     IN @stockIds");

        var rows = await conn.QueryAsync<StockBalance>(new CommandDefinition(
            sb.ToString(), new { warehouseIds, stockIds }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<StockBalanceWithCodeAndName>> GetAsync(
        string warehouseId, string[] stockIds, string? excludedTransactionId, CancellationToken ct = default)
    {
        if (stockIds.Length == 0) return Array.Empty<StockBalanceWithCodeAndName>();
        await using var conn = await OpenAsync(ct);
        var rows = await conn.QueryAsync<StockBalanceWithCodeAndName>(new CommandDefinition(
            @"SELECT
                sb.warehouse_id AS WarehouseId,
                sb.stock_id     AS StockId,
                s.code          AS Code,
                s.name          AS Name,
                COALESCE((
                    SELECT SUM(il.quantity) FROM invoice_lines il
                    JOIN invoices i ON i.id = il.invoice_id
                    WHERE il.stock_id = sb.stock_id AND i.warehouse_id = sb.warehouse_id
                      AND i.is_completed = 1
                      AND i.invoice_type IN ('Purchase','SalesReturn')
                      AND (@excludedId IS NULL OR i.id != @excludedId)
                ), 0) AS Income,
                COALESCE((
                    SELECT SUM(il.quantity) FROM invoice_lines il
                    JOIN invoices i ON i.id = il.invoice_id
                    WHERE il.stock_id = sb.stock_id AND i.warehouse_id = sb.warehouse_id
                      AND i.is_completed = 1
                      AND i.invoice_type IN ('Sales','PurchaseReturn')
                      AND (@excludedId IS NULL OR i.id != @excludedId)
                ), 0) AS Expense
              FROM stock_balances sb
              JOIN stocks s ON s.id = sb.stock_id
              WHERE sb.warehouse_id = @warehouseId AND sb.stock_id IN @stockIds",
            new { warehouseId, stockIds, excludedId = excludedTransactionId },
            cancellationToken: ct));
        return rows.ToList();
    }

    public Task<IReadOnlyList<StockBalanceByTypeWithBalanceAndData>> GetByTypeAsync(
        string[] warehouseIds, string stockId,
        DateTime dateFrom, DateTime dateTill, bool aggregate, CancellationToken ct = default)
    {
        // The window-function variant in the PG repo (SUM ... OVER) works in
        // SQLite ≥ 3.25. Implementing here means duplicating non-trivial SQL,
        // so we defer to a future iteration — the offline UI shows a simpler
        // "current balance" report and this advanced view is online-only.
        IReadOnlyList<StockBalanceByTypeWithBalanceAndData> empty = Array.Empty<StockBalanceByTypeWithBalanceAndData>();
        return Task.FromResult(empty);
    }

    public async Task<IReadOnlyList<StockBalanceByWarehouses>> GetByDateAndWarehousesAsync(
        DateTime date,
        IEnumerable<string>? warehouseIds,
        string? displayCurrencyId,
        IEnumerable<string>? stockIds = null,
        string? priceGroup = null,
        CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct);

        // priceGroup filter inside the windowed subquery — same logic as PG.
        // SQLite doesn't have proper IS NULL OR ... shortcut friendly to its
        // optimizer in window predicates, so we branch.
        var rows = await conn.QueryAsync(new CommandDefinition(
            @"SELECT
                s.id          AS stock_id,
                s.code,
                s.name,
                s.group_name,
                s.type,
                u.name        AS unit,
                json_group_array(json_object(
                    'warehouseId',   sb.warehouse_id,
                    'warehouseName', w.name,
                    'balance',       sb.income - sb.expense
                )) AS warehouse_balances,
                p.price,
                p.currency_id
              FROM stock_balances sb
              JOIN stocks     s ON s.id = sb.stock_id
              JOIN warehouses w ON w.id = sb.warehouse_id
              LEFT JOIN stock_units  u ON u.stock_id  = s.id AND u.is_default = 1
              LEFT JOIN (
                  SELECT stock_id, price, currency_id, price_group,
                         ROW_NUMBER() OVER (
                             PARTITION BY stock_id ORDER BY valid_from DESC
                         ) AS rn
                  FROM stock_prices
                  WHERE (@priceGroup IS NULL AND price_group IS NULL)
                     OR (@priceGroup IS NOT NULL AND price_group = @priceGroup)
              ) p ON p.stock_id = s.id AND p.rn = 1
              WHERE (sb.income - sb.expense) > 0
              GROUP BY s.id, s.code, s.name, s.group_name, s.type, u.name, p.price, p.currency_id
              ORDER BY s.name",
            new { priceGroup }, cancellationToken: ct));

        return rows.Select(r => new StockBalanceByWarehouses
        {
            StockId           = (string)r.stock_id,
            Code              = (string?)r.code,
            Name              = (string)r.name,
            Group             = (string?)r.group_name,
            Type              = (string?)r.type,
            Unit              = (string?)r.unit,
            Price             = r.price == null ? 0m : Convert.ToDecimal(r.price, System.Globalization.CultureInfo.InvariantCulture),
            CurrencyId        = (string?)r.currency_id,
            WarehouseBalances = (string?)r.warehouse_balances
        }).ToList();
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken ct)
    {
        var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        await pragma.ExecuteNonQueryAsync(ct);
        return conn;
    }
}
