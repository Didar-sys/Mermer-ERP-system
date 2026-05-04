using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Payhas.Binyat.Data.Postgres.Models;

namespace Payhas.Binyat.Data.Postgres.Abstractions;

/// <summary>
/// Stock CRUD + listing repository. Storage-agnostic — Postgres and SQLite
/// implementations both target this contract, which lets the Sync layer
/// swap them transparently.
/// </summary>
public interface IStocksRepository
{
    Task<Stock?> GetAsync(string id, CancellationToken ct = default);

    Task<IReadOnlyList<Stock>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns stocks in the same order as <paramref name="stockIds"/> (missing ids are skipped).</summary>
    Task<IReadOnlyList<Stock>> GetListAsync(string[] stockIds, CancellationToken ct = default);

    /// <summary>Lightweight projection for grid/list UIs — single SQL round-trip.</summary>
    Task<IReadOnlyList<StockInfo>> GetInfoAsync(string[]? stockIds = null, CancellationToken ct = default);

    /// <summary>Variant that joins additional-price-list values into the projection.</summary>
    Task<IReadOnlyList<StockInfo>> GetInfoAsync(
        string? additionalPriceCurrencyId,
        string? additionalPriceGroup,
        CancellationToken ct = default);

    Task<Stock> CreateAsync(Stock model, CancellationToken ct = default);
    Task<Stock> UpdateAsync(Stock model, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Merges all <paramref name="mergeStockIds"/> into <paramref name="mainStockId"/>
    /// (re-points <c>invoice_lines.stock_id</c>, optionally disables merged items
    /// and accumulates their barcodes onto the main item). Atomic.
    /// </summary>
    Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems, CancellationToken ct = default);

    /// <summary>Group/type facets for filter UI: { field → { value → count } }.</summary>
    Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>> GetFacetsAsync(string[] fields, CancellationToken ct = default);
}
