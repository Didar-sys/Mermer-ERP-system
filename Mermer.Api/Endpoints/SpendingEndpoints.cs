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

public static class SpendingEndpoints
{
    public static void MapSpendingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/spending/slips").WithTags("SpendingSlips");

        group.MapGet("", async (DateTime? from, DateTime? till, string? depositoryId, MermerDbContext db, CancellationToken ct) =>
        {
            var startDate = from ?? DateTime.MinValue;
            var endDate = till ?? DateTime.MaxValue;

            var defaultCur = await db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.IsDefault, ct)
                             ?? await db.Currencies.AsNoTracking().FirstOrDefaultAsync(ct);
            var defaultCurrencyId = defaultCur?.Id.ToString();

            var allConvertions = await GetCurrencyConvertionsAsync(db, DateTime.UtcNow, ct);

            var query = db.ExpenseSlips.Include(s => s.Lines).AsNoTracking().Where(s => s.Date >= startDate && s.Date <= endDate);

            if (Guid.TryParse(depositoryId, out var depGuid))
                query = query.Where(s => s.DepositoryId == depGuid);

            var slips = await query.OrderByDescending(s => s.Date).ToListAsync(ct);

            var result = slips.Select(s =>
            {
                var docCurrencyId = s.DisplayCurrencyId?.ToString() ?? defaultCurrencyId;
                decimal total = s.Lines?.Sum(l => l.Amount) ?? 0m;

                return new
                {
                    Id = s.Id.ToString(),
                    Code = s.Code ?? "",
                    Date = s.Date,
                    Type = "ExpenseSlip",
                    DepositoryId = s.DepositoryId?.ToString(),
                    DisplayCurrencyId = docCurrencyId,
                    CurrencyId = docCurrencyId,
                    CurrencyConvertions = allConvertions,
                    UserId = s.UserId?.ToString(),
                    UserName = s.UserName,
                    IsCompleted = s.IsCompleted,
                    IsDisabled = s.IsDisabled,
                    Group = s.GroupName ?? "",
                    Tags = s.Tags ?? Array.Empty<string>(),
                    Description = s.Description ?? "",
                    ActionTotal = total,
                    DisplayTotal = total,
                    LinesCount = s.Lines?.Count ?? 0,
                    Lines = s.Lines != null
                        ? s.Lines.Select(l => (object)new
                        {
                            Id = l.Id.ToString(),
                            ExpenseSlipId = s.Id.ToString(),
                            ExpenseId = l.ExpenseId?.ToString(),
                            Amount = l.Amount,
                            CurrencyId = l.CurrencyId?.ToString() ?? docCurrencyId,
                            SortOrder = l.SortOrder
                        }).ToList()
                        : new List<object>()
                };
            });

            return Results.Ok(result);
        });

        group.MapGet("/{id}", async (string id, MermerDbContext db, CancellationToken ct) =>
        {
            if (!Guid.TryParse(id, out var guid)) return Results.NotFound();
            var s = await db.ExpenseSlips.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == guid, ct);
            if (s == null) return Results.NotFound();

            var docCurrencyId = s.DisplayCurrencyId?.ToString() ?? (await db.Currencies.FirstOrDefaultAsync(c => c.IsDefault))?.Id.ToString();
            var convertions = await GetCurrencyConvertionsAsync(db, s.Date, ct);
            decimal total = s.Lines?.Sum(l => l.Amount) ?? 0m;

            return Results.Ok(new
            {
                Id = s.Id.ToString(),
                Code = s.Code ?? "",
                Date = s.Date,
                Type = "ExpenseSlip",
                DepositoryId = s.DepositoryId?.ToString(),
                DisplayCurrencyId = docCurrencyId,
                CurrencyId = docCurrencyId,
                CurrencyConvertions = convertions,
                UserId = s.UserId?.ToString(),
                UserName = s.UserName,
                IsCompleted = s.IsCompleted,
                IsDisabled = s.IsDisabled,
                Group = s.GroupName ?? "",
                Tags = s.Tags ?? Array.Empty<string>(),
                Description = s.Description ?? "",
                ActionTotal = total,
                DisplayTotal = total,
                LinesCount = s.Lines?.Count ?? 0,
                Lines = s.Lines != null
                    ? s.Lines.Select(l => (object)new
                    {
                        Id = l.Id.ToString(),
                        ExpenseSlipId = s.Id.ToString(),
                        ExpenseId = l.ExpenseId?.ToString(),
                        Amount = l.Amount,
                        CurrencyId = l.CurrencyId?.ToString() ?? docCurrencyId,
                        SortOrder = l.SortOrder
                    }).ToList()
                    : new List<object>()
            });
        });

        Func<HttpRequest, MermerDbContext, Task<IResult>> saveHandler = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            Guid id = Guid.TryParse(GetStringProp(root, "id", "Id"), out var g) && g != Guid.Empty ? g : Guid.NewGuid();
            var existing = await db.ExpenseSlips.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

            string code = GetStringProp(root, "code", "Code") ?? $"EXP-{DateTime.UtcNow:yyMMddHHmmss}";
            Guid? depId = Guid.TryParse(GetStringProp(root, "depositoryId", "DepositoryId"), out var dG) ? dG : null;
            Guid? curId = Guid.TryParse(GetStringProp(root, "displayCurrencyId", "DisplayCurrencyId", "currencyId", "CurrencyId"), out var cG) ? cG : null;
            Guid? userId = Guid.TryParse(GetStringProp(root, "userId", "UserId"), out var uG) ? uG : null;

            DateTime date = DateTime.UtcNow;
            if (DateTime.TryParse(GetStringProp(root, "date", "Date"), out var d)) date = d.ToUniversalTime();

            var linesList = new List<ExpenseSlipLineEntity>();
            if (TryGetPropCaseInsensitive(root, "lines", out var linesProp) && linesProp.ValueKind == JsonValueKind.Array)
            {
                int order = 0;
                foreach (var l in linesProp.EnumerateArray())
                {
                    linesList.Add(new ExpenseSlipLineEntity
                    {
                        Id = Guid.TryParse(GetStringProp(l, "id", "Id"), out var lId) && lId != Guid.Empty ? lId : Guid.NewGuid(),
                        ExpenseSlipId = id,
                        ExpenseId = Guid.TryParse(GetStringProp(l, "expenseId", "ExpenseId"), out var eG) ? eG : null,
                        Amount = GetDecimalProp(l, "amount", "Amount"),
                        CurrencyId = Guid.TryParse(GetStringProp(l, "currencyId", "CurrencyId"), out var lcG) ? lcG : curId,
                        SortOrder = order++
                    });
                }
            }

            if (existing == null)
            {
                await db.ExpenseSlips.AddAsync(new ExpenseSlipEntity
                {
                    Id = id,
                    Code = code,
                    Date = date,
                    UserId = userId,
                    DepositoryId = depId,
                    DisplayCurrencyId = curId,
                    UserName = GetStringProp(root, "userName", "UserName") ?? "admin",
                    IsCompleted = GetBoolProp(root, "isCompleted", "IsCompleted"),
                    IsDisabled = GetBoolProp(root, "isDisabled", "IsDisabled"),
                    GroupName = GetStringProp(root, "group", "Group") ?? "",
                    Description = GetStringProp(root, "description", "Description") ?? "",
                    Tags = Array.Empty<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Lines = linesList
                });
            }
            else
            {
                existing.Code = code;
                existing.Date = date;
                existing.UserId = userId;
                existing.DepositoryId = depId;
                existing.DisplayCurrencyId = curId;
                existing.IsCompleted = GetBoolProp(root, "isCompleted", "IsCompleted");
                existing.IsDisabled = GetBoolProp(root, "isDisabled", "IsDisabled");
                existing.GroupName = GetStringProp(root, "group", "Group") ?? "";
                existing.Description = GetStringProp(root, "description", "Description") ?? "";
                existing.UpdatedAt = DateTime.UtcNow;

                if (existing.Lines != null) db.ExpenseSlipLines.RemoveRange(existing.Lines);
                existing.Lines = linesList;
            }

            await db.SaveChangesAsync();
            return Results.Content($"{{\"id\":\"{id}\",\"code\":\"{code}\"}}", "application/json");
        };

        group.MapPost("", saveHandler);
        group.MapPut("/{id}", saveHandler);
        group.MapDelete("/{id}", async (string id, MermerDbContext db) =>
        {
            var item = await db.ExpenseSlips.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
            if (item != null)
            {
                db.ExpenseSlips.Remove(item);
                await db.SaveChangesAsync();
            }
            return Results.Ok();
        });

        group.MapGet("/facets", async (HttpContext ctx, MermerDbContext db, CancellationToken ct) =>
        {
            var now = DateTime.Now.Date;
            var slips = await db.ExpenseSlips.AsNoTracking().Where(s => !s.IsDisabled).Select(s => s.Date).ToListAsync(ct);
            var localDates = slips.Select(d => d.ToLocalTime().Date).ToList();

            return Results.Ok(new Dictionary<string, Dictionary<string, int>>
            {
                ["Date"] = new Dictionary<string, int>
                {
                    { "#Today", localDates.Count(d => d == now) },
                    { "#This Week", localDates.Count(d => d >= now.AddDays(-7)) },
                    { "#This Month", localDates.Count(d => d.Month == now.Month && d.Year == now.Year) },
                    { "#All Records", localDates.Count }
                }
            });
        });

        // ЖУРНАЛ СТАТЕЙ РАСХОДОВ (EXPENSE ACTIONS)
        routes.MapGet("/api/spending/actions", async (DateTime? from, DateTime? till, string? expenseId, HttpRequest req, MermerDbContext db, CancellationToken ct) =>
        {
            var startDate = from ?? DateTime.MinValue;
            var endDate = till ?? DateTime.MaxValue;

            var depIds = req.Query["depositoryId"]
                .Select(x => Guid.TryParse(x, out var g) ? (Guid?)g : null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            Guid? filterExpenseGuid = Guid.TryParse(expenseId, out var eGuid) ? eGuid : null;

            var query = db.ExpenseSlips
                .Include(s => s.Lines)
                .AsNoTracking()
                .Where(s => s.Date >= startDate && s.Date <= endDate && !s.IsDisabled);

            if (depIds.Any())
            {
                query = query.Where(s => s.DepositoryId.HasValue && depIds.Contains(s.DepositoryId.Value));
            }

            var slips = await query.OrderByDescending(s => s.Date).ToListAsync(ct);
            var actions = new List<object>();

            foreach (var s in slips)
            {
                foreach (var line in s.Lines ?? Enumerable.Empty<ExpenseSlipLineEntity>())
                {
                    // Фильтруем по конкретной статье расходов, если клиент передал expenseId
                    if (filterExpenseGuid.HasValue && line.ExpenseId != filterExpenseGuid) continue;

                    actions.Add(new
                    {
                        TransactionId = s.Id.ToString(),
                        TransactionCode = s.Code ?? "",
                        TransactionDate = s.Date,
                        TransactionType = "ExpenseSlip",
                        TransactionUserId = s.UserId?.ToString(),
                        TransactionUserName = s.UserName,
                        TransactionIsCompleted = s.IsCompleted,
                        TransactionIsDisabled = s.IsDisabled,
                        TransactionGroup = s.GroupName ?? "",
                        TransactionTags = s.Tags ?? Array.Empty<string>(),
                        ActionDepositoryId = s.DepositoryId?.ToString(),
                        ActionExpenseId = line.ExpenseId?.ToString(),
                        ActionAmount = line.Amount
                    });
                }
            }

            return Results.Ok(actions);
        }).WithTags("SpendingActions");
    }

    #region Helpers
    private static async Task<object[]> GetCurrencyConvertionsAsync(MermerDbContext db, DateTime docDate, CancellationToken ct)
    {
        var currencies = await db.Currencies.AsNoTracking().Where(c => !c.IsDisabled).ToListAsync(ct);
        var rates = await db.CurrencyRates
            .AsNoTracking()
            .Where(r => r.ValidFrom <= docDate.Date)
            .OrderByDescending(r => r.ValidFrom)
            .ToListAsync(ct);

        var convertions = new List<object>();
        foreach (var cur in currencies)
        {
            var rate = rates.FirstOrDefault(r => r.CurrencyId == cur.Id);
            decimal mult = rate?.Multiplier ?? 1m;
            decimal div = rate?.Divider ?? 1m;
            if (div == 0m) div = 1m;
            if (mult == 0m) mult = 1m;

            convertions.Add(new
            {
                CurrencyId = cur.Id.ToString(),
                Multiplier = mult,
                Divider = div
            });
        }
        return convertions.ToArray();
    }

    private static bool TryGetPropCaseInsensitive(JsonElement el, string name, out JsonElement val)
    {
        foreach (var p in el.EnumerateObject())
        {
            if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                val = p.Value;
                return true;
            }
        }
        val = default;
        return false;
    }

    private static string? GetStringProp(JsonElement el, params string[] names)
    {
        foreach (var n in names)
        {
            if (TryGetPropCaseInsensitive(el, n, out var p) && p.ValueKind == JsonValueKind.String)
                return p.GetString();
        }
        return null;
    }

    private static decimal GetDecimalProp(JsonElement el, params string[] names)
    {
        foreach (var n in names)
        {
            if (TryGetPropCaseInsensitive(el, n, out var p))
            {
                if (p.ValueKind == JsonValueKind.Number) return p.GetDecimal();
                if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var v)) return v;
            }
        }
        return 0m;
    }

    private static bool GetBoolProp(JsonElement el, params string[] names)
    {
        foreach (var n in names)
        {
            if (TryGetPropCaseInsensitive(el, n, out var p))
            {
                if (p.ValueKind == JsonValueKind.True) return true;
                if (p.ValueKind == JsonValueKind.False) return false;
            }
        }
        return false;
    }
    #endregion
}