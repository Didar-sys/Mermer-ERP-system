using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Entities;

namespace Mermer.Api.Endpoints;

public static class DepositoriesEndpoints
{
    public static void MapDepositoriesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/depositories").WithTags("Depositories");

        group.MapGet("/", async (MermerDbContext db) =>
        {
            var depositories = await db.Depositories
                .AsNoTracking()
                .Where(d => !d.IsDisabled)
                .ToListAsync();

            return Results.Ok(depositories);
        })
        .WithName("GetDepositories");

        // --- СОХРАНЕНИЕ КАССЫ (ДЛЯ СИНХРОНИЗАЦИИ ИЗ SQLITE) ---
        Func<HttpRequest, MermerDbContext, Task<IResult>> saveDepositoryHandler = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp) ? idProp.GetString() : null;
            Guid depId = Guid.TryParse(idStr, out var parsedGuid) ? parsedGuid : Guid.NewGuid();

            string name = root.TryGetProperty("name", out var nameProp) || root.TryGetProperty("Name", out nameProp) ? nameProp.GetString() : "Новая касса";
            string desc = root.TryGetProperty("description", out var descProp) || root.TryGetProperty("Description", out descProp) ? descProp.GetString() : "";

            var existing = await db.Depositories.FirstOrDefaultAsync(p => p.Id == depId);
            if (existing == null)
            {
                var entity = new DepositoryEntity
                {
                    Id = depId,
                    Name = name,
                    Description = desc,
                    IsDisabled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await db.Depositories.AddAsync(entity);
            }
            else
            {
                existing.Name = name;
                existing.Description = desc;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { id = depId });
        };

        group.MapPost("/", saveDepositoryHandler);
        group.MapPut("/{id}", saveDepositoryHandler);
    }
}