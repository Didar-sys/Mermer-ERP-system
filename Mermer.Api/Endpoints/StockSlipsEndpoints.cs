using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Mermer.Api.DTOs;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Entities;

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

        // --- СОХРАНЕНИЕ ---
        Func<HttpRequest, MermerDbContext, Task<IResult>> saveSlipHandler = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp) ? idProp.GetString() : null;
            Guid slipId = Guid.TryParse(idStr, out var parsedGuid) ? parsedGuid : Guid.NewGuid();

            string code = root.TryGetProperty("code", out var codeProp) || root.TryGetProperty("Code", out codeProp) ? codeProp.GetString() : $"SSLIP-{DateTime.UtcNow:yyMMddHHmmss}";
            string desc = root.TryGetProperty("description", out var descProp) || root.TryGetProperty("Description", out descProp) ? descProp.GetString() : "";

            DateTime date = DateTime.UtcNow;
            string dateStr = root.TryGetProperty("date", out var dProp) || root.TryGetProperty("Date", out dProp) ? dProp.GetString() : null;
            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var pDate)) date = pDate.ToUniversalTime();

            bool isCompleted = (root.TryGetProperty("isCompleted", out var comp) || root.TryGetProperty("IsCompleted", out comp)) && comp.ValueKind == JsonValueKind.True;

            var existing = await db.StockSlips.FirstOrDefaultAsync(p => p.Id == slipId);
            if (existing == null)
            {
                await db.StockSlips.AddAsync(new StockSlipEntity
                {
                    Id = slipId,
                    Code = code,
                    Date = date,
                    Description = desc,
                    IsCompleted = isCompleted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Code = code;
                existing.Date = date;
                existing.Description = desc;
                existing.IsCompleted = isCompleted;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { id = slipId, code });
        };

        // Обрабатываем POST и PUT по пути /api/catalog/slips
        group.MapPost("/slips", saveSlipHandler);
        group.MapPut("/slips/{id}", saveSlipHandler);
    }
}