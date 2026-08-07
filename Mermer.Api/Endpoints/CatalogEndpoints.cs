using Mermer.Api.DTOs;
using Mermer.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace Mermer.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/catalog").WithTags("Catalog");

        // --- КОНТРАГЕНТЫ (PARTNERS) ---
        group.MapGet("/partners", async (MermerDbContext db) =>
        {
            var partners = await db.Partners
                .AsNoTracking()
                .Select(p => new PartnerDetailsDto(
                    p.Id.ToString(),
                    p.Name
                ))
                .ToListAsync();

            return Results.Ok(partners);
        })
        .WithName("GetPartners")
        .WithSummary("Получить список контрагентов");
    }
}