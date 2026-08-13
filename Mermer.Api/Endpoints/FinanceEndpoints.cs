using System;
using System.Collections.Generic;
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

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/finance").WithTags("Finance");

        group.MapGet("/slips", async (MermerDbContext db) =>
        {
            var slips = await db.FundsSlips
                .Include(s => s.Lines)
                .AsNoTracking()
                .ToListAsync();

            // Добавляем опцию для игнорирования бесконечных циклов
            var jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Чтобы регистр свойств совпадал
            };

            return Results.Json(slips, jsonOptions);
        });

        routes.MapGet("/api/currencies", async (MermerDbContext db) =>
        {
            var currencies = await db.Currencies.AsNoTracking().ToListAsync();
            return Results.Ok(currencies);
        });

        group.MapPost("/slips", async (HttpRequest request, MermerDbContext db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(body))
                return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = GetStringProperty(root, "id", "Id");
            Guid slipId = Guid.TryParse(idStr, out var parsedGuid) ? parsedGuid : Guid.NewGuid();

            var existing = await db.FundsSlips
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s => s.Id == slipId);

            string code = GetStringProperty(root, "code", "Code") ?? $"FS-{DateTime.UtcNow:yyMMddHHmmss}";
            string depIdStr = GetStringProperty(root, "depositoryId", "DepositoryId");
            Guid? depId = Guid.TryParse(depIdStr, out var parsedDep) ? parsedDep : null;

            DateTime date = DateTime.UtcNow;
            string dateStr = GetStringProperty(root, "date", "Date");
            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
            {
                date = parsedDate.ToUniversalTime();
            }

            string rawType = GetStringProperty(root, "slipType", "SlipType", "fundsSlipType") ?? "";
            string dbSlipType = "Income";
            if (rawType.Contains("Expense", StringComparison.OrdinalIgnoreCase) ||
                rawType.Contains("Deficit", StringComparison.OrdinalIgnoreCase) ||
                rawType == "2")
            {
                dbSlipType = "Expense";
            }

            var linesList = new List<FundsSlipLineEntity>();
            if (TryGetPropertyCaseInsensitive(root, "lines", out var linesProp) && linesProp.ValueKind == JsonValueKind.Array)
            {
                int sortOrder = 0;
                foreach (var lineJson in linesProp.EnumerateArray())
                {
                    decimal amount = 0m;
                    if (TryGetPropertyCaseInsensitive(lineJson, "amount", out var amProp))
                    {
                        if (amProp.ValueKind == JsonValueKind.Number) amount = amProp.GetDecimal();
                        else if (amProp.ValueKind == JsonValueKind.String) decimal.TryParse(amProp.GetString(), out amount);
                    }

                    string curIdStr = GetStringProperty(lineJson, "currencyId", "CurrencyId");
                    Guid? currencyGuid = Guid.TryParse(curIdStr, out var cG) ? cG : null;

                    string lineIdStr = GetStringProperty(lineJson, "id", "Id");
                    Guid lineGuid = Guid.TryParse(lineIdStr, out var lG) ? lG : Guid.NewGuid();

                    linesList.Add(new FundsSlipLineEntity
                    {
                        Id = lineGuid,
                        FundsSlipId = slipId,
                        Amount = amount,
                        CurrencyId = currencyGuid,
                        SortOrder = sortOrder++
                    });
                }
            }

            if (existing == null)
            {
                var entity = new FundsSlipEntity
                {
                    Id = slipId,
                    Code = code,
                    Date = date,
                    FundsSlipType = dbSlipType,
                    DepositoryId = depId,
                    IsCompleted = GetBoolProperty(root, "isCompleted", "IsCompleted"),
                    IsDisabled = GetBoolProperty(root, "isDisabled", "IsDisabled"),
                    UserName = GetStringProperty(root, "userName", "UserName") ?? "admin",
                    Group = GetStringProperty(root, "group", "Group") ?? "",
                    Description = GetStringProperty(root, "description", "Description") ?? "",
                    Tags = Array.Empty<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Lines = linesList
                };

                await db.FundsSlips.AddAsync(entity);
            }
            else
            {
                existing.Code = code;
                existing.Date = date;
                existing.FundsSlipType = dbSlipType;
                existing.DepositoryId = depId;
                existing.IsCompleted = GetBoolProperty(root, "isCompleted", "IsCompleted");
                existing.IsDisabled = GetBoolProperty(root, "isDisabled", "IsDisabled");
                existing.Description = GetStringProperty(root, "description", "Description") ?? "";
                existing.UpdatedAt = DateTime.UtcNow;

                if (existing.Tags == null) existing.Tags = Array.Empty<string>();

                if (existing.Lines != null)
                {
                    db.FundsSlipLines.RemoveRange(existing.Lines);
                }
                existing.Lines = linesList;
            }

            await db.SaveChangesAsync();

            return Results.Content($"{{\"id\":\"{slipId}\",\"code\":\"{code}\"}}", "application/json");
        })
        .WithName("CreateFundsSlip");
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

    private static string GetStringProperty(JsonElement element, params string[] propNames)
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