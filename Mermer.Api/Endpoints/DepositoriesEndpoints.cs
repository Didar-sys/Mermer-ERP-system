using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;

namespace Mermer.Api.Endpoints;

public static class DepositoriesEndpoints
{
    public static void MapDepositoriesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/depositories").WithTags("Depositories");

        group.MapGet("/", async (MermerDbContext db) =>
        {
            // Отдаем чистые объекты прямо из БД, без анонимных типов
            var depositories = await db.Depositories
                .AsNoTracking()
                .Where(d => !d.IsDisabled)
                .ToListAsync();

            return Results.Ok(depositories);
        })
        .WithName("GetDepositories");
    }
}