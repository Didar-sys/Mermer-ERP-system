using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Payhas.Binyat.Data.Postgres.Repositories;

namespace Payhas.Binyat.Data.Postgres.Abstractions;

/// <summary>
/// Fuzzy / full-text stock search.
/// PostgreSQL implementation uses pg_trgm + tsvector + LATERAL joins
/// in a single round-trip.
/// </summary>
public interface IStockSearchService
{
    Task<IReadOnlyList<PgStockSearchResult>> SearchAsync(
        string searchText,
        string? warehouseId = null,
        string? priceGroup = null,
        int limit = 32,
        double minSimilarity = 0.1,
        CancellationToken cancellationToken = default);
}
