using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Payhas.Binyat.Data.Postgres.Repositories;

/// <summary>
/// Interface for PostgreSQL-backed stock search service.
/// Replaces Couchbase FTS (StockSearchServiceOld) with pg_trgm fuzzy search.
/// Target: < 100ms response time for all queries.
/// </summary>
public interface IPgStockSearchService
{
    /// <summary>
    /// Performs fuzzy search on stocks by name, code, short_name, and barcodes.
    /// Uses pg_trgm GIN indexes for trigram-based similarity matching.
    /// Single SQL query replaces the 3 sequential Couchbase round-trips.
    /// </summary>
    /// <param name="searchText">User input (may contain typos)</param>
    /// <param name="warehouseId">Warehouse to get balance from (nullable for all warehouses)</param>
    /// <param name="priceGroup">Optional price group filter</param>
    /// <param name="limit">Max results (default 32 to match old behavior)</param>
    /// <param name="minSimilarity">Minimum trigram similarity threshold (default 0.1)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<PgStockSearchResult>> SearchAsync(
        string searchText,
        string? warehouseId = null,
        string? priceGroup = null,
        int limit = 32,
        double minSimilarity = 0.1,
        CancellationToken cancellationToken = default);
}
