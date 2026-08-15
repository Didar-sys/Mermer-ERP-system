using System.Text.Json;
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

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };

        group.MapGet("/", async (MermerDbContext db) =>
        {
            var depositories = await db.Depositories.AsNoTracking().Where(d => !d.IsDisabled).ToListAsync();
            var result = depositories.Select(d => new
            {
                Id = d.Id.ToString(),
                Name = d.Name,
                OfficeId = d.OfficeId?.ToString(),
                IsDisabled = d.IsDisabled
            });
            return Results.Json(result, jsonOptions);
        });

        group.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.Depositories.CountAsync();
            var nextCode = $"DEP-{(count + 1):D5}";
            return Results.Ok(new { code = nextCode });
        });
    }
}