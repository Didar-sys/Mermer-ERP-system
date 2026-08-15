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

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this IEndpointRouteBuilder routes)
    {
        // 1. ОБРАБОТЧИК ПОЛУЧЕНИЯ СПИСКА (GET)
        Func<DateTime?, DateTime?, string?, string?, MermerDbContext, CancellationToken, Task<IResult>> getSlipsHandler =
            async (from, till, depositoryId, partnerId, db, ct) =>
            {
                var startDate = from ?? DateTime.MinValue;
                var endDate = till ?? DateTime.MaxValue;

                var query = db.FundsSlips
                    .Include(s => s.Lines)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .Where(s => s.Date >= startDate && s.Date <= endDate);

                if (Guid.TryParse(depositoryId, out var depGuid))
                    query = query.Where(s => s.DepositoryId == depGuid);

                if (Guid.TryParse(partnerId, out var partGuid))
                    query = query.Where(s => s.PartnerId == partGuid);

                var slips = await query.OrderByDescending(s => s.Date).ToListAsync(ct);

                var result = slips.Select(s =>
                {
                    // Безопасный маппинг для клиента
                    string billTypeValue = "Collection";
                    if (!string.IsNullOrEmpty(s.FundsSlipType) &&
                        (s.FundsSlipType.Equals("Collection", StringComparison.OrdinalIgnoreCase) ||
                         s.FundsSlipType.Equals("Income", StringComparison.OrdinalIgnoreCase)))
                    {
                        billTypeValue = "Collection";
                    }

                    return new
                    {
                        Id = s.Id.ToString(),
                        Code = s.Code ?? string.Empty,
                        Date = s.Date,
                        FundsSlipType = s.FundsSlipType,
                        SlipType = s.FundsSlipType,
                        BillType = billTypeValue,
                        Type = billTypeValue,
                        OfficeId = s.OfficeId?.ToString(),
                        DepositoryId = s.DepositoryId?.ToString(),
                        PartnerId = s.PartnerId?.ToString(),
                        UserName = s.UserName,
                        IsCompleted = s.IsCompleted,
                        IsDisabled = s.IsDisabled,
                        Description = s.Description ?? string.Empty,

                        Total = s.Lines != null ? s.Lines.Sum(l => l.Amount) : 0m,
                        DisplayTotal = s.Lines != null ? s.Lines.Sum(l => l.Amount) : 0m,
                        Amount = s.Lines != null ? s.Lines.Sum(l => l.Amount) : 0m,

                        Lines = s.Lines != null && s.Lines.Any()
                            ? s.Lines.Select(l => (object)new
                            {
                                Id = l.Id.ToString(),
                                Amount = l.Amount,
                                CurrencyId = l.CurrencyId?.ToString(),
                                SortOrder = l.SortOrder
                            })
                            : Array.Empty<object>()
                    };
                });

                return Results.Ok(result);
            };

        // 2. ОБРАБОТЧИК СОХРАНЕНИЯ (POST / PUT)
        Func<HttpRequest, MermerDbContext, Task<IResult>> saveSlipHandler = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(body))
                return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = GetStringProperty(root, "id", "Id");
            Guid slipId = Guid.TryParse(idStr, out var parsedGuid) && parsedGuid != Guid.Empty ? parsedGuid : Guid.NewGuid();

            var existing = await db.FundsSlips
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s => s.Id == slipId);

            string code = GetStringProperty(root, "code", "Code") ?? $"DOC-{DateTime.UtcNow:yyMMddHHmmss}";

            string depIdStr = GetStringProperty(root, "depositoryId", "DepositoryId");
            Guid? depId = Guid.TryParse(depIdStr, out var parsedDep) ? parsedDep : null;

            string partIdStr = GetStringProperty(root, "partnerId", "PartnerId");
            Guid? partId = Guid.TryParse(partIdStr, out var parsedPart) ? parsedPart : null;

            string offIdStr = GetStringProperty(root, "officeId", "OfficeId");
            Guid? offId = Guid.TryParse(offIdStr, out var parsedOff) ? parsedOff : null;

            DateTime date = DateTime.UtcNow;
            string dateStr = GetStringProperty(root, "date", "Date");
            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
            {
                date = parsedDate.ToUniversalTime();
            }

            // ИСПРАВЛЕНИЕ: Маппинг клиентского типа в тип БД, чтобы не нарушать CHECK CONSTRAINT
            string rawType = GetStringProperty(root, "billType", "BillType", "slipType", "SlipType", "fundsSlipType", "type", "Type") ?? "Collection";
            string dbSlipType = "Income"; // По умолчанию приход

            if (rawType.Equals("Payment", StringComparison.OrdinalIgnoreCase) ||
                rawType.Contains("Expense", StringComparison.OrdinalIgnoreCase) ||
                rawType.Contains("Deficit", StringComparison.OrdinalIgnoreCase))
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
                    Guid lineGuid = Guid.TryParse(lineIdStr, out var lG) && lG != Guid.Empty ? lG : Guid.NewGuid();

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
                    PartnerId = partId,
                    OfficeId = offId,
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
                existing.PartnerId = partId;
                existing.OfficeId = offId;
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
        };

        // Регистрируем роуты без лишних слэшей
        var financeGroup = routes.MapGroup("/api/finance").WithTags("Finance");
        financeGroup.MapGet("/slips", getSlipsHandler);
        financeGroup.MapPost("/slips", saveSlipHandler);
        financeGroup.MapPut("/slips/{id}", saveSlipHandler);

        var billsGroup = routes.MapGroup("/api/bills").WithTags("Bills");
        billsGroup.MapGet("", getSlipsHandler);
        billsGroup.MapPost("", saveSlipHandler);
        billsGroup.MapPut("/{id}", saveSlipHandler);

        billsGroup.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.FundsSlips.CountAsync();
            return Results.Ok(new { code = $"DOC-{DateTime.UtcNow:yyMMdd}{(count + 1):D4}" });
        });

        // Эндпоинт для счетчиков
        billsGroup.MapGet("/facets", async (HttpContext context, MermerDbContext db, CancellationToken ct) =>
        {
            // Получаем поля из строки запроса
            string fields = context.Request.Query["fields"].ToString();
            var fieldList = string.IsNullOrEmpty(fields)
                ? new[] { "Date" }
                : fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var result = new Dictionary<string, Dictionary<string, int>>();

            foreach (var field in fieldList)
            {
                result[field] = new Dictionary<string, int>();
            }

            if (fieldList.Contains("Date", StringComparer.OrdinalIgnoreCase) || fieldList.Contains("transaction", StringComparer.OrdinalIgnoreCase))
            {
                // ИСПРАВЛЕНИЕ 2: Используем локальное время для честного сравнения дат
                var now = DateTime.Now.Date;
                var slips = await db.FundsSlips.AsNoTracking().Where(s => !s.IsDisabled).Select(s => s.Date).ToListAsync(ct);

                // Переводим все даты из базы (UTC) в локальное время сервера
                var localDates = slips.Select(d => d.ToLocalTime().Date).ToList();

                var dateFacets = new Dictionary<string, int>
                {
                    { "#Today", localDates.Count(d => d == now) },
                    { "#Yesturday", localDates.Count(d => d == now.AddDays(-1)) },
                    { "#This Week", localDates.Count(d => d >= now.AddDays(-7)) },
                    { "#Past Week", localDates.Count(d => d >= now.AddDays(-14) && d < now.AddDays(-7)) },
                    { "#This Month", localDates.Count(d => d.Month == now.Month && d.Year == now.Year) },
                    { "#Past Month", localDates.Count(d => d.Month == now.AddMonths(-1).Month && d.Year == now.AddMonths(-1).Year) },
                    { "#This Year", localDates.Count(d => d.Year == now.Year) },
                    { "#All Records", localDates.Count }
                };

                result["Date"] = dateFacets;
                result["date"] = dateFacets; // Дублируем в нижнем регистре для страховки
            }

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