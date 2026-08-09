using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Abstractions;
using Microsoft.EntityFrameworkCore;

// Псевдонимы для точного разделения моделей
using UIStock = Mermer.StockManagement.Models.Stock;
using UIStockInfo = Mermer.StockManagement.Models.StockInfo;
using PgStock = Mermer.Data.Postgres.Models.Stock;

namespace Mermer.Api.Endpoints;

public static class StocksEndpoints
{
    public static IEndpointRouteBuilder MapStocksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stocks").WithTags("Stocks");

        // --- 1. СПИСОК ТОВАРОВ ДЛЯ КЛИЕНТА (ОТДАЕМ В ФОРМАТЕ UIStockInfo) ---
        group.MapGet("/", async (
            string? additionalPriceCurrencyId,
            string? additionalPriceGroup,
            MermerDbContext db,
            CancellationToken ct) =>
        {
            var list = await db.Stocks
                .AsNoTracking()
                .Select(s => new UIStockInfo
                {
                    Id = s.Id.ToString(),
                    Code = s.Code ?? string.Empty,
                    Name = s.Name ?? string.Empty,
                    ShortName = s.ShortName ?? string.Empty,
                    IsDisabled = s.IsDisabled,
                    Unit = s.Units.Select(u => u.Name).FirstOrDefault() ?? string.Empty,
                    Price = s.Prices.Select(p => p.Price).FirstOrDefault(),
                    CurrencyId = s.Prices.Select(p => p.CurrencyId.HasValue ? p.CurrencyId.Value.ToString() : null).FirstOrDefault(),
                    Type = s.Type,
                    Group = s.Group,
                    Barcodes = s.Barcodes ?? new string[0],
                    Tags = s.Tags ?? new string[0]
                })
                .ToListAsync(ct);

            return Results.Ok(list);
        })
        .WithName("StocksList")
        .WithSummary("Lightweight stock list (info projection).");

        // --- 2. ПОИСК ТОВАРОВ ---
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
        .WithName("StocksSearch");

        // --- 3. АВТОНУМЕРАТОР ТОВАРОВ ---
        group.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.Stocks.CountAsync();
            var nextCode = $"ST-{(count + 1):D6}";
            return Results.Ok(new { code = nextCode });
        })
        .WithName("StocksGetNextCode");

        // --- 4. ТОВАР ПО ID ---
        group.MapGet("/{id}", async (
            string id,
            IStocksRepository repo,
            CancellationToken ct) =>
        {
            var stock = await repo.GetAsync(id, ct);
            return stock is null ? Results.NotFound() : Results.Ok(stock);
        })
        .WithName("StocksGetById");

        // --- 5. ФАСЕТЫ ---
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
        .WithName("StocksFacets");

        // --- 6. СОЗДАНИЕ (POST) ---
        group.MapPost("/", async (
            UIStock model,
            IStocksRepository repo,
            CancellationToken ct) =>
        {
            var pgModel = MapToPgStock(model);
            var created = await repo.CreateAsync(pgModel, ct);
            return Results.Created($"/api/stocks/{created.Id}", created);
        })
        .WithName("StocksCreate");

        // --- 7. ОБНОВЛЕНИЕ (PUT) ---
        group.MapPut("/{id}", async (
            string id,
            UIStock model,
            IStocksRepository repo,
            CancellationToken ct) =>
        {
            model.Id = id;
            var pgModel = MapToPgStock(model);
            var updated = await repo.UpdateAsync(pgModel, ct);
            return Results.Ok(updated);
        })
        .WithName("StocksUpdate");

        // --- 8. УДАЛЕНИЕ (DELETE) ---
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

    // Вспомогательный маппер между UIStock и PgStock
    private static PgStock MapToPgStock(UIStock src)
    {
        return new PgStock
        {
            Id = src.Id,
            Code = src.Code,
            Name = src.Name,
            ShortName = src.ShortName,
            Type = src.Type,
            Group = src.Group,
            IsDisabled = src.IsDisabled,

            // Исправлено: ToArray() заменено на ToList() для соответствия типам PostgreSQL
            Barcodes = src.Barcodes?.ToList(),
            Tags = src.Tags?.ToList()

            // Поля Price, Unit и CurrencyId удалены, 
            // так как в БД они должны сохраняться через связанные коллекции (Prices / Units).
        };
    }
}