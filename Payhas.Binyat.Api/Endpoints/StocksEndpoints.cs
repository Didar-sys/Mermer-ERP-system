using Payhas.Binyat.Data.Postgres.Abstractions;
using Payhas.Binyat.Data.Postgres.Models;

namespace Payhas.Binyat.Api.Endpoints;

/// <summary>
/// Stock-related endpoints: search (fuzzy + full-text), single fetch,
/// list-with-info projection. Search is the hot path that must stay
/// under ~100 ms even on hundreds of thousands of items.
/// </summary>
public static class StocksEndpoints
{
    public static IEndpointRouteBuilder MapStocksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stocks").WithTags("Stocks");

        group.MapGet("/search", async (
            string q,
            string? warehouseId,
            string? priceGroup,
            int? limit,
            double? minSimilarity,
            IStockSearchService search,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest(new { error = "Query parameter 'q' is required." });

            var result = await search.SearchAsync(
                searchText: q,
                warehouseId: warehouseId,
                priceGroup: priceGroup,
                limit: limit ?? 32,
                minSimilarity: minSimilarity ?? 0.1,
                cancellationToken: ct);

            return Results.Ok(result);
        })
        .WithName("StocksSearch")
        .WithSummary("Fuzzy search by code/barcode/name (pg_trgm + tsvector).");

        group.MapGet("/", async (
            string? additionalPriceCurrencyId,
            string? additionalPriceGroup,
            IStocksRepository repo,
            CancellationToken ct) =>
        {
            var info = (additionalPriceCurrencyId is not null || additionalPriceGroup is not null)
                ? await repo.GetInfoAsync(additionalPriceCurrencyId, additionalPriceGroup, ct)
                : await repo.GetInfoAsync(null, ct);

            return Results.Ok(info);
        })
        .WithName("StocksList")
        .WithSummary("Lightweight stock list (info projection).");

        group.MapGet("/{id}", async (
            string id,
            IStocksRepository repo,
            CancellationToken ct) =>
        {
            var stock = await repo.GetAsync(id, ct);
            return stock is null ? Results.NotFound() : Results.Ok(stock);
        })
        .WithName("StocksGetById")
        .WithSummary("Full stock with units, prices and additional prices.");

        group.MapGet("/facets", async (
            string fields,
            IStocksRepository repo,
            CancellationToken ct) =>
        {
            var fieldList = fields
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (fieldList.Length == 0)
                return Results.BadRequest(new { error = "Provide at least one field, e.g. ?fields=type,group" });

            var facets = await repo.GetFacetsAsync(fieldList, ct);
            return Results.Ok(facets);
        })
        .WithName("StocksFacets")
        .WithSummary("Filter UI facets — value→count per field.");

        group.MapPost("/", async (
            Stock model,
            IStocksRepository repo,
            CancellationToken ct) =>
        {
            var created = await repo.CreateAsync(model, ct);
            return Results.Created($"/api/stocks/{created.Id}", created);
        })
        .WithName("StocksCreate");

        group.MapPut("/{id}", async (
            string id,
            Stock model,
            IStocksRepository repo,
            CancellationToken ct) =>
        {
            model.Id = id;
            var updated = await repo.UpdateAsync(model, ct);
            return Results.Ok(updated);
        })
        .WithName("StocksUpdate");

        group.MapDelete("/{id}", async (
            string id,
            IStocksRepository repo,
            CancellationToken ct) =>
        {
            await repo.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("StocksDelete");

        return app;
    }
}
