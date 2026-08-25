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

public static class ExpensesEndpoints
{
    public static void MapExpensesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/expenses").WithTags("Expenses");

        // 1. Получение списка всех статей
        group.MapGet("/", async (MermerDbContext db, CancellationToken ct) =>
        {
            var expenses = await db.Expenses.AsNoTracking().Where(e => !e.IsDisabled).ToListAsync(ct);
            var result = expenses.Select(e => new
            {
                Id = e.Id.ToString(),
                Name = e.Name ?? string.Empty,
                Type = e.Type ?? string.Empty,
                Group = e.Group ?? string.Empty,
                Description = e.Description ?? string.Empty,
                Tags = e.Tags != null ? e.Tags.ToList() : new List<string>(),
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
                Tags = e.Tags != null ? e.Tags.ToList() : new List<string>(),
                IsDisabled = e.IsDisabled
            });
        });

        // 3. Создание / сохранение новой статьи
        Func<HttpRequest, MermerDbContext, CancellationToken, Task<IResult>> saveExpenseHandler = async (request, db, ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync(ct);
            if (string.IsNullOrWhiteSpace(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string? idStr = GetStringProperty(root, "id", "Id");
            Guid entityId = Guid.TryParse(idStr, out var parsedGuid) && parsedGuid != Guid.Empty ? parsedGuid : Guid.NewGuid();

            var tagsList = ExtractTagsFromRawJson(root);

            string name = GetStringProperty(root, "name", "Name") ?? string.Empty;
            string? type = GetStringProperty(root, "type", "Type");
            string? groupName = GetStringProperty(root, "group", "Group");
            string? description = GetStringProperty(root, "description", "Description");
            bool isDisabled = GetBoolProperty(root, "isDisabled", "IsDisabled");

            var existing = await db.Expenses.FirstOrDefaultAsync(e => e.Id == entityId, ct);
            if (existing == null)
            {
                var entity = new ExpenseEntity
                {
                    Id = entityId,
                    Name = name,
                    Type = type,
                    Group = groupName,
                    Description = description,
                    IsDisabled = isDisabled,
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
                    Tags = entity.Tags != null ? entity.Tags.ToList() : new List<string>(),
                    IsDisabled = entity.IsDisabled
                });
            }
            else
            {
                existing.Name = name;
                existing.Type = type;
                existing.Group = groupName;
                existing.Description = description;
                existing.IsDisabled = isDisabled;
                existing.Tags = tagsList.ToArray();
                existing.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync(ct);

                return Results.Ok(new
                {
                    Id = existing.Id.ToString(),
                    Name = existing.Name,
                    Type = existing.Type ?? string.Empty,
                    Group = existing.Group ?? string.Empty,
                    Description = existing.Description ?? string.Empty,
                    Tags = existing.Tags != null ? existing.Tags.ToList() : new List<string>(),
                    IsDisabled = existing.IsDisabled
                });
            }
        };

        group.MapPost("/", saveExpenseHandler);
        group.MapPut("/{id}", saveExpenseHandler);

        // 4. Удаление статьи
        group.MapDelete("/{id}", async (string id, MermerDbContext db, CancellationToken ct) =>
        {
            if (Guid.TryParse(id, out var guidId))
            {
                var existing = await db.Expenses.FindAsync(new object[] { guidId }, ct);
                if (existing != null)
                {
                    existing.IsDisabled = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync(ct);
                }
            }
            return Results.NoContent();
        });

        // 5. Фасеты для выпадающих списков (TypeNames, GroupNames, TagNames)
        group.MapGet("/facets", async (HttpContext context, MermerDbContext db, CancellationToken ct) =>
        {
            var result = new Dictionary<string, Dictionary<string, int>>();
            var all = await db.Expenses.AsNoTracking().Where(e => !e.IsDisabled).ToListAsync(ct);

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

            var tagDict = all.Where(x => x.Tags != null && x.Tags.Length > 0)
                             .SelectMany(x => x.Tags!)
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .GroupBy(x => x)
                             .ToDictionary(g => g.Key, g => g.Count());

            result["TypeNames"] = typeDict;
            result["GroupNames"] = groupDict;
            result["TagNames"] = tagDict;
            result["Tags"] = tagDict;
            result["Group"] = groupDict;
            result["Type"] = typeDict;

            return Results.Ok(result);
        });
    }

    #region Helpers
    private static List<string> ExtractTagsFromRawJson(JsonElement root)
    {
        var list = new List<string>();

        if (!root.TryGetProperty("tags", out var tagsProp) &&
            !root.TryGetProperty("Tags", out tagsProp))
        {
            return list;
        }

        if (tagsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in tagsProp.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var s = item.GetString();
                    if (!string.IsNullOrWhiteSpace(s)) list.Add(s.Trim());
                }
                else if (item.ValueKind == JsonValueKind.Object)
                {
                    if (item.TryGetProperty("Text", out var t) || item.TryGetProperty("Value", out t) || item.TryGetProperty("Name", out t))
                    {
                        var s = t.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) list.Add(s.Trim());
                    }
                }
            }
        }
        else if (tagsProp.ValueKind == JsonValueKind.String)
        {
            var raw = tagsProp.GetString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                list.AddRange(raw.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(x => x.Trim())
                                 .Where(x => !string.IsNullOrWhiteSpace(x)));
            }
        }

        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

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