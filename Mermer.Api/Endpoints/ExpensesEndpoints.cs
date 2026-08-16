using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mermer.Api.Endpoints;

public static class ExpensesEndpoints
{
    public static void MapExpensesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/expenses").WithTags("Expenses");

        // 1. Получение списка всех статей
        group.MapGet("/", async (MermerDbContext db, CancellationToken ct) =>
        {
            var expenses = await db.Expenses.AsNoTracking().ToListAsync(ct);
            var result = expenses.Select(e => new
            {
                Id = e.Id.ToString(),
                Name = e.Name ?? string.Empty,
                Type = e.Type ?? string.Empty,
                Group = e.Group ?? string.Empty,
                Description = e.Description ?? string.Empty,
                Tags = e.Tags ?? Array.Empty<string>(),
                IsDisabled = e.IsDisabled
            });
            return Results.Ok(result);
        });

        // 2. Получение одной статьи по ID
        group.MapGet("/{id}", async (string id, MermerDbContext db, CancellationToken ct) =>
        {
            if (!Guid.TryParse(id, out var guidId))
                return Results.NotFound();

            var e = await db.Expenses.FindAsync(new object[] { guidId }, ct);
            if (e == null) return Results.NotFound();

            return Results.Ok(new
            {
                Id = e.Id.ToString(),
                Name = e.Name ?? string.Empty,
                Type = e.Type ?? string.Empty,
                Group = e.Group ?? string.Empty,
                Description = e.Description ?? string.Empty,
                Tags = e.Tags ?? Array.Empty<string>(),
                IsDisabled = e.IsDisabled
            });
        });

        // 3. Создание новой статьи
        group.MapPost("/", async (HttpRequest request, MermerDbContext db, CancellationToken ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync(ct);
            if (string.IsNullOrWhiteSpace(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string? idStr = GetStringProperty(root, "id", "Id");
            Guid entityId = Guid.TryParse(idStr, out var parsedGuid) && parsedGuid != Guid.Empty ? parsedGuid : Guid.NewGuid();

            var tagsList = new List<string>();
            if (TryGetPropertyCaseInsensitive(root, "tags", out var tagsProp) && tagsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var tag in tagsProp.EnumerateArray())
                {
                    if (tag.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(tag.GetString()))
                        tagsList.Add(tag.GetString()!);
                }
            }

            var entity = new ExpenseEntity
            {
                Id = entityId,
                Name = GetStringProperty(root, "name", "Name") ?? string.Empty,
                Type = GetStringProperty(root, "type", "Type"),
                Group = GetStringProperty(root, "group", "Group"),
                Description = GetStringProperty(root, "description", "Description"),
                IsDisabled = GetBoolProperty(root, "isDisabled", "IsDisabled"),
                Tags = tagsList.ToArray(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await db.Expenses.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                Type = entity.Type ?? string.Empty,
                Group = entity.Group ?? string.Empty,
                Description = entity.Description ?? string.Empty,
                Tags = entity.Tags ?? Array.Empty<string>(),
                IsDisabled = entity.IsDisabled
            });
        });

        // 4. Обновление существующей статьи
        group.MapPut("/{id}", async (string id, HttpRequest request, MermerDbContext db, CancellationToken ct) =>
        {
            if (!Guid.TryParse(id, out var guidId))
                return Results.NotFound();

            var existing = await db.Expenses.FindAsync(new object[] { guidId }, ct);
            if (existing == null) return Results.NotFound();

            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync(ct);
            if (!string.IsNullOrWhiteSpace(body))
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var tagsList = new List<string>();
                if (TryGetPropertyCaseInsensitive(root, "tags", out var tagsProp) && tagsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tag in tagsProp.EnumerateArray())
                    {
                        if (tag.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(tag.GetString()))
                            tagsList.Add(tag.GetString()!);
                    }
                }

                existing.Name = GetStringProperty(root, "name", "Name") ?? existing.Name;
                existing.Type = GetStringProperty(root, "type", "Type") ?? existing.Type;
                existing.Group = GetStringProperty(root, "group", "Group") ?? existing.Group;
                existing.Description = GetStringProperty(root, "description", "Description");
                existing.IsDisabled = GetBoolProperty(root, "isDisabled", "IsDisabled");
                existing.Tags = tagsList.ToArray();
                existing.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync(ct);
            }

            return Results.Ok(new
            {
                Id = existing.Id.ToString(),
                Name = existing.Name,
                Type = existing.Type ?? string.Empty,
                Group = existing.Group ?? string.Empty,
                Description = existing.Description ?? string.Empty,
                Tags = existing.Tags ?? Array.Empty<string>(),
                IsDisabled = existing.IsDisabled
            });
        });

        // 5. Удаление статьи
        group.MapDelete("/{id}", async (string id, MermerDbContext db, CancellationToken ct) =>
        {
            if (Guid.TryParse(id, out var guidId))
            {
                var existing = await db.Expenses.FindAsync(new object[] { guidId }, ct);
                if (existing != null)
                {
                    db.Expenses.Remove(existing);
                    await db.SaveChangesAsync(ct);
                }
            }
            return Results.NoContent();
        });

        // 6. Фасеты для выпадающих списков
        group.MapGet("/facets", async (HttpContext context, MermerDbContext db, CancellationToken ct) =>
        {
            var result = new Dictionary<string, Dictionary<string, int>>();
            var all = await db.Expenses.AsNoTracking().ToListAsync(ct);

            var typeDict = all.Where(x => !string.IsNullOrWhiteSpace(x.Type))
                              .GroupBy(x => x.Type!)
                              .ToDictionary(g => g.Key, g => g.Count());

            var defaultTypes = new[] { "Operating", "Administrative", "Commercial", "Financial", "Other" };
            foreach (var dt in defaultTypes)
            {
                if (!typeDict.ContainsKey(dt)) typeDict[dt] = 0;
            }

            var groupDict = all.Where(x => !string.IsNullOrWhiteSpace(x.Group))
                               .GroupBy(x => x.Group!)
                               .ToDictionary(g => g.Key, g => g.Count());

            var defaultGroups = new[] { "General", "Office", "Rent", "Salary", "Logistics", "Marketing", "Taxes" };
            foreach (var dg in defaultGroups)
            {
                if (!groupDict.ContainsKey(dg)) groupDict[dg] = 0;
            }

            result["TypeNames"] = typeDict;
            result["GroupNames"] = groupDict;
            result["TagNames"] = new Dictionary<string, int>();

            return Results.Ok(result);
        });
    }

    #region Helpers
    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string propName, out JsonElement value)
    {
        foreach (var prop in element.EnumerateObject())
        {
            if (string.Equals(prop.Name, propName, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private static string? GetStringProperty(JsonElement element, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (TryGetPropertyCaseInsensitive(element, name, out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString();
            }
        }
        return null;
    }

    private static bool GetBoolProperty(JsonElement element, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (TryGetPropertyCaseInsensitive(element, name, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.True) return true;
                if (prop.ValueKind == JsonValueKind.False) return false;
            }
        }
        return false;
    }
    #endregion
}