using System;
using System.Collections.Generic;
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
using Mermer.Data.Postgres.Entities;

namespace Mermer.Api.Endpoints;

public static class StockOrderTemplatesEndpoints
{
    public static IEndpointRouteBuilder MapStockOrderTemplatesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warehousing/order-templates").WithTags("StockOrderTemplates");

        JsonElement GetProp(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var val)) return val;
            if (el.TryGetProperty(char.ToLower(name[0]) + name.Substring(1), out val)) return val;
            return default;
        }

        group.MapGet("/", async (MermerDbContext db, CancellationToken ct) =>
        {
            var list = await db.StockOrderTemplates
                .Include(t => t.Lines)
                .AsSplitQuery()
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(ct);

            return Results.Ok(list.Select(t => new
            {
                Id = t.Id.ToString(),
                Name = t.Name,
                IsDisabled = t.IsDisabled,
                Group = t.GroupName ?? "",
                Tags = t.Tags ?? Array.Empty<string>(),
                Description = t.Description ?? "",
                Lines = t.Lines.Select(l => new
                {
                    Id = l.Id.ToString(),
                    StockId = l.StockId?.ToString()
                })
            }));
        });

        group.MapGet("/facets", async (string? fields, MermerDbContext db, CancellationToken ct) =>
        {
            var dict = new Dictionary<string, Dictionary<string, int>>();

            // Загружаем фасеты Групп
            var allGroups = await db.StockOrderTemplates
                .Where(x => x.GroupName != null && x.GroupName != "")
                .GroupBy(x => x.GroupName!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToListAsync(ct);
            dict["GroupNames"] = allGroups.ToDictionary(x => x.Key, x => x.Count);

            // Загружаем фасеты Тегов
            var tagsList = await db.StockOrderTemplates
                .Where(x => x.Tags != null)
                .Select(x => x.Tags)
                .ToListAsync(ct);

            var tagCounts = tagsList.SelectMany(t => t!)
                .GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count());
            dict["TagNames"] = tagCounts;

            return Results.Ok(dict);
        });

        group.MapPost("/", async (HttpRequest req, MermerDbContext db) =>
        {
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var ip = GetProp(root, "Id");
            Guid templateId = ip.ValueKind != JsonValueKind.Undefined && Guid.TryParse(ip.GetString(), out var parsedGuid) && parsedGuid != Guid.Empty ? parsedGuid : Guid.NewGuid();

            var existing = await db.StockOrderTemplates.Include(t => t.Lines).AsSplitQuery().FirstOrDefaultAsync(r => r.Id == templateId);
            if (existing == null)
            {
                existing = new StockOrderTemplateEntity { Id = templateId, CreatedAt = DateTimeOffset.UtcNow };
                await db.StockOrderTemplates.AddAsync(existing);
            }

            var np = GetProp(root, "Name");
            if (np.ValueKind != JsonValueKind.Undefined) existing.Name = np.GetString() ?? "";

            var dis = GetProp(root, "IsDisabled");
            if (dis.ValueKind == JsonValueKind.True || dis.ValueKind == JsonValueKind.False) existing.IsDisabled = dis.GetBoolean();

            var gp = GetProp(root, "Group");
            if (gp.ValueKind != JsonValueKind.Undefined) existing.GroupName = gp.GetString();

            var dp = GetProp(root, "Description");
            if (dp.ValueKind != JsonValueKind.Undefined) existing.Description = dp.GetString();

            // Сохранение тегов
            var tagsElem = GetProp(root, "Tags");
            if (tagsElem.ValueKind == JsonValueKind.Array)
            {
                existing.Tags = tagsElem.EnumerateArray()
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray()!;
            }
            else if (tagsElem.ValueKind == JsonValueKind.Null)
            {
                existing.Tags = null;
            }

            existing.UpdatedAt = DateTimeOffset.UtcNow;

            var linesElem = GetProp(root, "Lines");
            if (linesElem.ValueKind == JsonValueKind.Array)
            {
                db.StockOrderTemplateLines.RemoveRange(existing.Lines);
                foreach (var le in linesElem.EnumerateArray())
                {
                    var lidProp = GetProp(le, "Id");
                    var lStockProp = GetProp(le, "StockId");

                    var line = new StockOrderTemplateLineEntity
                    {
                        Id = lidProp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(lidProp.GetString(), out var lG) ? lG : Guid.NewGuid(),
                        StockOrderTemplateId = templateId,
                        StockId = lStockProp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(lStockProp.GetString(), out var stG) ? stG : null
                    };
                    await db.StockOrderTemplateLines.AddAsync(line);
                }
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { id = templateId });
        });

        group.MapDelete("/{id}", async (string id, MermerDbContext db) =>
        {
            if (Guid.TryParse(id, out var guid))
            {
                var o = await db.StockOrderTemplates.FirstOrDefaultAsync(x => x.Id == guid);
                if (o != null)
                {
                    db.StockOrderTemplates.Remove(o);
                    await db.SaveChangesAsync();
                }
            }
            return Results.Ok();
        });

        return app;
    }
}