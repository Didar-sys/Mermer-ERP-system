using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Abstractions;
using Mermer.Data.Postgres.Entities;

// Псевдонимы для точного разделения моделей
using UIStockInfo = Mermer.StockManagement.Models.StockInfo;

namespace Mermer.Api.Endpoints;

public static class StocksEndpoints
{
    public static IEndpointRouteBuilder MapStocksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stocks").WithTags("Stocks");

        group.MapGet("/", async (string? additionalPriceCurrencyId, string? additionalPriceGroup, MermerDbContext db, CancellationToken ct) =>
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
        .WithName("StocksList");

        group.MapGet("/search", async (string q, string? warehouseId, string? priceGroup, int? limit, double? minSimilarity, IStockSearchService search, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q)) return Results.BadRequest(new { error = "Query parameter 'q' is required." });
            var result = await search.SearchAsync(q, warehouseId, priceGroup, limit ?? 32, minSimilarity ?? 0.1, ct);
            return Results.Ok(result);
        })
        .WithName("StocksSearch");

        group.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.Stocks.CountAsync();
            return Results.Ok(new { code = $"ST-{(count + 1):D6}" });
        })
        .WithName("StocksGetNextCode");

        group.MapGet("/{id}", async (string id, IStocksRepository repo, CancellationToken ct) =>
        {
            var stock = await repo.GetAsync(id, ct);
            return stock is null ? Results.NotFound() : Results.Ok(stock);
        })
        .WithName("StocksGetById");

        group.MapGet("/facets", async (string fields, IStocksRepository repo, CancellationToken ct) =>
        {
            var fieldList = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (fieldList.Length == 0) return Results.BadRequest(new { error = "Provide fields." });
            var facets = await repo.GetFacetsAsync(fieldList, ct);
            return Results.Ok(facets);
        })
        .WithName("StocksFacets");

        // --- СОХРАНЕНИЕ ---
        Func<HttpRequest, MermerDbContext, Task<IResult>> saveStockHandler = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp) ? idProp.GetString() : null;
            Guid stockId = Guid.TryParse(idStr, out var parsedGuid) ? parsedGuid : Guid.NewGuid();

            string code = root.TryGetProperty("code", out var codeProp) || root.TryGetProperty("Code", out codeProp) ? codeProp.GetString() : $"ST-{DateTime.UtcNow:yyMMddHHmmss}";
            string name = root.TryGetProperty("name", out var nameProp) || root.TryGetProperty("Name", out nameProp) ? nameProp.GetString() : "Новый товар";
            string type = root.TryGetProperty("type", out var typeProp) || root.TryGetProperty("Type", out typeProp) ? typeProp.GetString() : "";

            var existing = await db.Stocks.FirstOrDefaultAsync(p => p.Id == stockId);
            if (existing == null)
            {
                await db.Stocks.AddAsync(new StockEntity
                {
                    Id = stockId,
                    Code = code,
                    Name = name,
                    Type = type,
                    IsDisabled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Code = code;
                existing.Name = name;
                existing.Type = type;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { id = stockId, code });
        };

        group.MapPost("/", saveStockHandler);
        group.MapPut("/{id}", saveStockHandler);

        group.MapDelete("/{id}", async (string id, IStocksRepository repo, CancellationToken ct) =>
        {
            await repo.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("StocksDelete");

        return app;
    }
}