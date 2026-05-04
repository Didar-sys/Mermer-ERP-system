using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Payhas.Binyat.Data.Postgres.Models;

namespace Payhas.Binyat.Data.Postgres.Abstractions;

/// <summary>
/// Stock balance queries — current and historical (point-in-time).
///
/// Point-in-time balances are recomputed directly from <c>invoice_lines</c>
/// (NOT from the <c>stock_balances</c> running-total table) because the
/// running totals can't answer questions about a past date.
/// </summary>
public interface IStockBalancesRepository
{
    /// <summary>Balance for one stock across one or more warehouses, at <paramref name="date"/>.</summary>
    Task<IReadOnlyList<StockBalance>> GetAsync(
        string stockId, DateTime date, string[]? warehouseIds = null, CancellationToken ct = default);

    /// <summary>Balances for several stocks in one warehouse (current totals).</summary>
    Task<IReadOnlyList<StockBalance>> GetAsync(
        string warehouseId, string[] stockIds, CancellationToken ct = default);

    /// <summary>Balances for several stocks across several warehouses (current totals).</summary>
    Task<IReadOnlyList<StockBalance>> GetAsync(
        string[] warehouseIds, string[] stockIds, CancellationToken ct = default);

    /// <summary>
    /// "Before this transaction" balance — used in the transaction edit UI
    /// to show stock on hand excluding the lines of the current document.
    /// </summary>
    Task<IReadOnlyList<StockBalanceWithCodeAndName>> GetAsync(
        string warehouseId, string[] stockIds, string? excludedTransactionId, CancellationToken ct = default);

    /// <summary>Per-day running balance for a stock across warehouses, by invoice type.</summary>
    Task<IReadOnlyList<StockBalanceByTypeWithBalanceAndData>> GetByTypeAsync(
        string[] warehouseIds, string stockId,
        DateTime dateFrom, DateTime dateTill, bool aggregate, CancellationToken ct = default);

    /// <summary>
    /// Aggregated balance report: current stock with per-warehouse JSON
    /// breakdown.
    /// </summary>
    /// <param name="priceGroup">
    /// Optional price-list selector (e.g. "wholesale", "retail").
    /// When set, the report column "Price" reflects the matching
    /// <c>stock_prices</c> entry; when null, the default (no-group) price
    /// is returned.
    /// </param>
    Task<IReadOnlyList<StockBalanceByWarehouses>> GetByDateAndWarehousesAsync(
        DateTime date,
        IEnumerable<string>? warehouseIds,
        string? displayCurrencyId,
        IEnumerable<string>? stockIds = null,
        string? priceGroup = null,
        CancellationToken ct = default);
}
