using Mermer.Api.DTOs;
using Mermer.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace Mermer.Api.Endpoints;

public static class StockSlipsEndpoints
{
    public static void MapStockSlipsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/catalog").WithTags("Catalog");

        // --- СКЛАДСКИЕ ОРДЕРА (STOCK SLIPS) ---
        group.MapGet("/slips", async (MermerDbContext db) =>
        {
            var slips = await db.StockSlips
                .AsNoTracking()
                .Select(s => new StockSlipDto(
                    s.Id.ToString(),
                    s.Code,
                    s.SlipType,
                    s.Date.UtcDateTime,
                    s.IsCompleted,
                    s.DisplayTotal,
                    s.Description
                ))
                .ToListAsync();

            return Results.Ok(slips);
        })
        .WithName("GetStockSlips")
        .WithSummary("Получить список складских ордеров");
    }
}