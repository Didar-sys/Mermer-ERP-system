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

public static class PartnersEndpoints
{
    public static void MapPartnersEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/partners").WithTags("Partners");

        group.MapGet("/", async (MermerDbContext db) =>
        {
            var partners = await db.Partners.AsNoTracking().Where(p => !p.IsDisabled).ToListAsync();
            return Results.Ok(partners);
        });

        group.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.Partners.CountAsync();
            var nextCode = $"P-{(count + 1):D5}";
            return Results.Ok(new { code = nextCode });
        });

        // 2. Расчет балансов партнёров по операциям
        group.MapGet("/balances/by-type", async (string? partnerId, DateTime? from, DateTime? till, string[]? officeIds, MermerDbContext db) =>
        {
            var partnersQuery = db.Partners.AsNoTracking().Where(p => !p.IsDisabled);
            if (!string.IsNullOrEmpty(partnerId) && Guid.TryParse(partnerId, out var pGuid))
            {
                partnersQuery = partnersQuery.Where(p => p.Id == pGuid);
            }
            var partners = await partnersQuery.ToListAsync();

            var resultList = new List<object>();

            foreach (var partner in partners)
            {
                // 1. Накладные (Invoices)
                var invoicesQuery = db.Invoices.Include(i => i.Lines).AsNoTracking().Where(i => i.PartnerId == partner.Id);
                if (from.HasValue) invoicesQuery = invoicesQuery.Where(i => i.Date >= from.Value.ToUniversalTime());
                if (till.HasValue) invoicesQuery = invoicesQuery.Where(i => i.Date <= till.Value.ToUniversalTime());
                var invoices = await invoicesQuery.ToListAsync();

                decimal sales = invoices.Where(i => i.InvoiceType == "Sales").Sum(i => i.Lines?.Sum(l => l.Quantity * l.Price) ?? 0m);
                decimal purchases = invoices.Where(i => i.InvoiceType == "Purchase").Sum(i => i.Lines?.Sum(l => l.Quantity * l.Price) ?? 0m);
                decimal salesReturn = invoices.Where(i => i.InvoiceType == "SalesReturn").Sum(i => i.Lines?.Sum(l => l.Quantity * l.Price) ?? 0m);
                decimal purchaseReturn = invoices.Where(i => i.InvoiceType == "PurchaseReturn").Sum(i => i.Lines?.Sum(l => l.Quantity * l.Price) ?? 0m);

                // 2. Документы взаиморасчетов (PartnerSlips)
                var slipsQuery = db.PartnerSlips.Include(s => s.Lines).AsNoTracking();
                if (from.HasValue) slipsQuery = slipsQuery.Where(s => s.Date >= from.Value.ToUniversalTime());
                if (till.HasValue) slipsQuery = slipsQuery.Where(s => s.Date <= till.Value.ToUniversalTime());
                var slips = await slipsQuery.ToListAsync();

                decimal opening = slips
                    .Where(s => s.SlipType == "PartnerOpeningBalance")
                    .SelectMany(s => s.Lines ?? new List<PartnerSlipLineEntity>())
                    .Where(l => l.PartnerId == partner.Id)
                    .Sum(l => l.DebitAmount - l.CreditAmount);

                decimal revision = slips
                    .Where(s => s.SlipType == "PartnerBalanceRevision")
                    .SelectMany(s => s.Lines ?? new List<PartnerSlipLineEntity>())
                    .Where(l => l.PartnerId == partner.Id)
                    .Sum(l => l.DebitAmount - l.CreditAmount);

                // 3. Формула итогового баланса
                decimal resultingBalance = opening + revision + sales - salesReturn - purchases + purchaseReturn;

                resultList.Add(new
                {
                    PartnerId = partner.Id.ToString(),
                    OfficeId = Guid.Empty.ToString(),
                    StartingBalance = opening,
                    Opening = opening,
                    Revision = revision,
                    Transfer = 0m,
                    Sales = sales,
                    SalesReturn = salesReturn,
                    Purchase = purchases,
                    PurchaseReturn = purchaseReturn,
                    Payment = 0m,
                    Collection = 0m,
                    ResultingBalance = resultingBalance
                });
            }

            return Results.Ok(resultList);
        });

        group.MapGet("/balances", async (MermerDbContext db) =>
        {
            return Results.Ok(new object[] { });
        });

        // 3. Сохранение партнера
        Func<HttpRequest, MermerDbContext, Task<IResult>> savePartnerHandler = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp) ? idProp.GetString() : null;
            Guid partnerId = Guid.TryParse(idStr, out var parsedGuid) ? parsedGuid : Guid.NewGuid();

            string code = root.TryGetProperty("code", out var codeProp) || root.TryGetProperty("Code", out codeProp) ? codeProp.GetString() : $"P-{DateTime.UtcNow:yyMMddHHmmss}";
            string name = root.TryGetProperty("name", out var nameProp) || root.TryGetProperty("Name", out nameProp) ? nameProp.GetString() : "Новый партнер";
            string phone = root.TryGetProperty("phone", out var phoneProp) || root.TryGetProperty("Phone", out phoneProp) ? phoneProp.GetString() : "";

            var existing = await db.Partners.FirstOrDefaultAsync(p => p.Id == partnerId);
            if (existing == null)
            {
                var entity = new PartnerEntity
                {
                    Id = partnerId,
                    Code = code,
                    Name = name,
                    Phone = phone,
                    IsDisabled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await db.Partners.AddAsync(entity);
            }
            else
            {
                existing.Code = code;
                existing.Name = name;
                existing.Phone = phone;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.Content($"{{\"id\":\"{partnerId}\",\"code\":\"{code}\"}}", "application/json");
        };

        group.MapPost("/", savePartnerHandler);
        routes.MapPost("/api/catalog/partners", savePartnerHandler);

        // 4. Подгрузка документов PartnerSlips
        group.MapGet("/slips", async (MermerDbContext db) =>
        {
            var slips = await db.PartnerSlips
                .Include(s => s.Lines)
                .AsNoTracking()
                .ToListAsync();

            var result = slips.Select(s => new
            {
                Id = s.Id.ToString(),
                Code = s.Code,
                Date = s.Date,
                SlipType = s.SlipType == "PartnerOpeningBalance" ? 0 : 1,
                Type = s.SlipType,
                OfficeId = s.OfficeId?.ToString(),
                IsDisabled = s.IsDisabled,
                IsCompleted = true,
                DocType = "PartnerSlip",
                DebitTotal = s.Lines?.Sum(l => l.DebitAmount) ?? 0m,
                CreditTotal = s.Lines?.Sum(l => l.CreditAmount) ?? 0m,
                Lines = s.Lines != null && s.Lines.Any()
                    ? s.Lines.Select(l => (object)new
                    {
                        Id = l.Id.ToString(),
                        PartnerId = l.PartnerId?.ToString(),
                        DebitAmount = l.DebitAmount,
                        DebitCurrencyId = l.DebitCurrencyId?.ToString(),
                        CreditAmount = l.CreditAmount,
                        CreditCurrencyId = l.CreditCurrencyId?.ToString()
                    }).ToList()
                    : new List<object>(),
                CurrencyConvertions = new List<object>()
            });

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // Фиксируем PascalCase
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            return Results.Json(result, jsonOptions);
        });

        // 5. Сохранение документов PartnerSlips
        group.MapPost("/slips", async (HttpRequest request, MermerDbContext db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = GetJsonString(root, "id", "Id");
            Guid slipId = Guid.TryParse(idStr, out var parsedGuid) && parsedGuid != Guid.Empty ? parsedGuid : Guid.NewGuid();

            string code = GetJsonString(root, "code", "Code") ?? $"DOC-{DateTime.UtcNow:yyMMddHHmmss}";
            string slipType = GetJsonString(root, "type", "Type", "slipType", "SlipType") ?? "PartnerOpeningBalance";

            string offStr = GetJsonString(root, "officeId", "OfficeId");
            Guid? officeGuid = Guid.TryParse(offStr, out var parsedOff) ? parsedOff : null;

            string dateStr = GetJsonString(root, "date", "Date");
            DateTime slipDate = DateTime.TryParse(dateStr, out var pDate) ? pDate.ToUniversalTime() : DateTime.UtcNow;

            var existing = await db.PartnerSlips.FirstOrDefaultAsync(s => s.Id == slipId);

            var linesList = new List<PartnerSlipLineEntity>();
            if (TryGetPropertyCaseInsensitive(root, "lines", out var linesProp) && linesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var l in linesProp.EnumerateArray())
                {
                    string partnerIdStr = GetJsonString(l, "partnerId", "PartnerId");
                    Guid? pGuid = Guid.TryParse(partnerIdStr, out var parsedP) ? parsedP : null;

                    string debitCurStr = GetJsonString(l, "debitCurrencyId", "DebitCurrencyId");
                    Guid? debitCurGuid = Guid.TryParse(debitCurStr, out var pDebCur) ? pDebCur : null;

                    string creditCurStr = GetJsonString(l, "creditCurrencyId", "CreditCurrencyId");
                    Guid? creditCurGuid = Guid.TryParse(creditCurStr, out var pCredCur) ? pCredCur : null;

                    decimal debit = GetJsonDecimal(l, "debitAmount", "DebitAmount");
                    decimal credit = GetJsonDecimal(l, "creditAmount", "CreditAmount");

                    linesList.Add(new PartnerSlipLineEntity
                    {
                        Id = Guid.NewGuid(),
                        PartnerSlipId = slipId,
                        PartnerId = pGuid,
                        DebitAmount = debit,
                        DebitCurrencyId = debitCurGuid,
                        CreditAmount = credit,
                        CreditCurrencyId = creditCurGuid
                    });
                }
            }

            // Только сохраняем шапку, чтобы Entity Framework не ругался на линии
            if (existing == null)
            {
                var entity = new PartnerSlipEntity
                {
                    Id = slipId,
                    Code = code,
                    Date = slipDate,
                    SlipType = slipType,
                    OfficeId = officeGuid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await db.PartnerSlips.AddAsync(entity);
                await db.SaveChangesAsync();
            }
            else
            {
                existing.Code = code;
                existing.Date = slipDate;
                existing.SlipType = slipType;
                existing.OfficeId = officeGuid;
                existing.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            // Удаляем старые строки жестким SQL-запросом и вставляем новые
            await db.Database.ExecuteSqlRawAsync("DELETE FROM partner_slip_lines WHERE partner_slip_id = {0}", slipId);

            if (linesList.Any())
            {
                await db.PartnerSlipLines.AddRangeAsync(linesList);
                await db.SaveChangesAsync();
            }

            return Results.Content($"{{\"id\":\"{slipId}\",\"code\":\"{code}\"}}", "application/json");
        });

        // 7. Подгрузка документов PartnerTransfers с реальными курсами валют
        group.MapGet("/transfers", async (MermerDbContext db) =>
        {
            var transfers = await db.PartnerTransfers
                .Include(t => t.Lines)
                .AsNoTracking()
                .ToListAsync();

            // Загружаем все курсы валют из PostgreSQL
            var allRates = await db.CurrencyRates.AsNoTracking().ToListAsync();

            var result = transfers.Select(t =>
            {
                // Собираем все утилизированные валюты из документа
                var usedCurrencyIds = t.Lines != null
                    ? t.Lines.Select(l => l.DebitCurrencyId)
                             .Union(t.Lines.Select(l => l.CreditCurrencyId))
                             .Where(c => c.HasValue)
                             .Select(c => c!.Value)
                             .Distinct()
                             .ToList()
                    : new List<Guid>();

                // Формируем список конвертаций с актуальными курсами из БД
                var currencyConvertions = usedCurrencyIds.Select(cId =>
                {
                    var rate = allRates
                        .Where(r => r.CurrencyId == cId && r.ValidFrom <= t.Date) // Просто сравниваем DateTime с DateTime
                        .OrderByDescending(r => r.ValidFrom)
                        .FirstOrDefault();

                    return (object)new
                    {
                        Id = Guid.NewGuid().ToString(),
                        CurrencyId = cId.ToString(),
                        Multiplier = rate?.Multiplier ?? 1m,
                        Divider = rate?.Divider ?? 1m
                    };
                }).ToList();

                return new
                {
                    Id = t.Id.ToString(),
                    Code = t.Code,
                    Date = t.Date,
                    Type = "PartnerTransfer",
                    IsDisabled = t.IsDisabled,
                    IsCompleted = true,
                    DocType = "PartnerTransfer",
                    Lines = t.Lines != null && t.Lines.Any()
                        ? t.Lines.Select(l => (object)new
                        {
                            Id = l.Id.ToString(),
                            OfficeId = l.OfficeId?.ToString(),
                            PartnerId = l.PartnerId?.ToString(),
                            DebitAmount = l.DebitAmount,
                            DebitCurrencyId = l.DebitCurrencyId?.ToString(),
                            CreditAmount = l.CreditAmount,
                            CreditCurrencyId = l.CreditCurrencyId?.ToString()
                        }).ToList()
                        : new List<object>(),
                    CurrencyConvertions = currencyConvertions
                };
            });

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            return Results.Json(result, jsonOptions);
        });

        // 8. Сохранение переводов PartnerTransfers
        group.MapPost("/transfers", async (HttpRequest request, MermerDbContext db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = GetJsonString(root, "id", "Id");
            Guid transferId = Guid.TryParse(idStr, out var parsedGuid) && parsedGuid != Guid.Empty ? parsedGuid : Guid.NewGuid();

            string code = GetJsonString(root, "code", "Code") ?? $"DOC-{DateTime.UtcNow:yyMMddHHmmss}";
            string dateStr = GetJsonString(root, "date", "Date");
            DateTime transferDate = DateTime.TryParse(dateStr, out var pDate) ? pDate.ToUniversalTime() : DateTime.UtcNow;

            var existing = await db.PartnerTransfers.FirstOrDefaultAsync(t => t.Id == transferId);

            var linesList = new List<PartnerTransferLineEntity>();
            if (TryGetPropertyCaseInsensitive(root, "lines", out var linesProp) && linesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var l in linesProp.EnumerateArray())
                {
                    string officeIdStr = GetJsonString(l, "officeId", "OfficeId");
                    Guid? oGuid = Guid.TryParse(officeIdStr, out var parsedO) ? parsedO : null;

                    string partnerIdStr = GetJsonString(l, "partnerId", "PartnerId");
                    Guid? pGuid = Guid.TryParse(partnerIdStr, out var parsedP) ? parsedP : null;

                    string debitCurStr = GetJsonString(l, "debitCurrencyId", "DebitCurrencyId");
                    Guid? debitCurGuid = Guid.TryParse(debitCurStr, out var pDebCur) ? pDebCur : null;

                    string creditCurStr = GetJsonString(l, "creditCurrencyId", "CreditCurrencyId");
                    Guid? creditCurGuid = Guid.TryParse(creditCurStr, out var pCredCur) ? pCredCur : null;

                    decimal debit = GetJsonDecimal(l, "debitAmount", "DebitAmount");
                    decimal credit = GetJsonDecimal(l, "creditAmount", "CreditAmount");

                    linesList.Add(new PartnerTransferLineEntity
                    {
                        Id = Guid.NewGuid(),
                        PartnerTransferId = transferId,
                        OfficeId = oGuid,
                        PartnerId = pGuid,
                        DebitAmount = debit,
                        DebitCurrencyId = debitCurGuid,
                        CreditAmount = credit,
                        CreditCurrencyId = creditCurGuid
                    });
                }
            }

            if (existing == null)
            {
                var entity = new PartnerTransferEntity
                {
                    Id = transferId,
                    Code = code,
                    Date = transferDate,
                    Lines = linesList,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await db.PartnerTransfers.AddAsync(entity);
                await db.SaveChangesAsync();
            }
            else
            {
                existing.Code = code;
                existing.Date = transferDate;
                existing.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            await db.Database.ExecuteSqlRawAsync("DELETE FROM partner_transfer_lines WHERE partner_transfer_id = {0}", transferId);

            if (linesList.Any())
            {
                await db.PartnerTransferLines.AddRangeAsync(linesList);
                await db.SaveChangesAsync();
            }

            return Results.Content($"{{\"id\":\"{transferId}\",\"code\":\"{code}\"}}", "application/json");
        });

        // 6. Реестр движений по партнерам (PartnerActions)
        group.MapGet("/actions", async (string? partnerId, DateTime? from, DateTime? till, string[]? officeIds, MermerDbContext db) =>
        {
            var actionsList = new List<PartnerActionDto>();

            // 1. Подтягиваем накладные (Invoices)
            var invoicesQuery = db.Invoices.Include(i => i.Lines).AsNoTracking();
            if (!string.IsNullOrEmpty(partnerId) && Guid.TryParse(partnerId, out var pGuid))
                invoicesQuery = invoicesQuery.Where(i => i.PartnerId == pGuid);
            if (from.HasValue) invoicesQuery = invoicesQuery.Where(i => i.Date >= from.Value.ToUniversalTime());
            if (till.HasValue) invoicesQuery = invoicesQuery.Where(i => i.Date <= till.Value.ToUniversalTime());

            var invoices = await invoicesQuery.ToListAsync();

            foreach (var inv in invoices)
            {
                decimal total = inv.Lines?.Sum(l => l.Quantity * l.Price) ?? 0m;
                bool isSales = inv.InvoiceType == "Sales" || inv.InvoiceType == "PurchaseReturn";

                actionsList.Add(new PartnerActionDto
                {
                    TransactionId = inv.Id.ToString(),
                    TransactionCode = inv.Code ?? "DOC",
                    TransactionType = inv.InvoiceType ?? "Sales",
                    TransactionDate = inv.Date.DateTime, // Для DateTimeOffset берем .DateTime
                    ActionOfficeId = inv.OfficeId?.ToString() ?? Guid.Empty.ToString(),
                    ActionPartnerId = inv.PartnerId?.ToString() ?? string.Empty,
                    ActionDebit = isSales ? total : 0m,
                    ActionCredit = isSales ? 0m : total,
                    ActionEffect = isSales ? total : -total,
                    TransactionUserName = inv.UserName ?? "admin",
                    TransactionIsCompleted = inv.IsCompleted,
                    TransactionIsDisabled = inv.IsDisabled
                });
            }

            // 2. Подтягиваем акты взаиморасчетов (PartnerSlips)
            var slipsQuery = db.PartnerSlips.Include(s => s.Lines).AsNoTracking();
            if (from.HasValue) slipsQuery = slipsQuery.Where(s => s.Date >= from.Value.ToUniversalTime());
            if (till.HasValue) slipsQuery = slipsQuery.Where(s => s.Date <= till.Value.ToUniversalTime());

            var slips = await slipsQuery.ToListAsync();

            foreach (var slip in slips)
            {
                if (slip.Lines == null) continue;

                foreach (var line in slip.Lines)
                {
                    if (!string.IsNullOrEmpty(partnerId) && line.PartnerId.ToString() != partnerId)
                        continue;

                    actionsList.Add(new PartnerActionDto
                    {
                        TransactionId = slip.Id.ToString(),
                        TransactionCode = slip.Code ?? "DOC",
                        TransactionType = slip.SlipType ?? "PartnerOpeningBalance",
                        TransactionDate = slip.Date, // slip.Date уже является DateTime!
                        ActionOfficeId = slip.OfficeId?.ToString() ?? Guid.Empty.ToString(),
                        ActionPartnerId = line.PartnerId?.ToString() ?? string.Empty,
                        ActionDebit = line.DebitAmount,
                        ActionCredit = line.CreditAmount,
                        ActionEffect = line.DebitAmount - line.CreditAmount,
                        TransactionUserName = "admin",
                        TransactionIsCompleted = true,
                        TransactionIsDisabled = slip.IsDisabled
                    });
                }
            }

            // 3. Подтягиваем переводы (PartnerTransfers)
            var transfersQuery = db.PartnerTransfers.Include(t => t.Lines).AsNoTracking();
            if (from.HasValue) transfersQuery = transfersQuery.Where(t => t.Date >= from.Value.ToUniversalTime());
            if (till.HasValue) transfersQuery = transfersQuery.Where(t => t.Date <= till.Value.ToUniversalTime());

            var transfers = await transfersQuery.ToListAsync();

            foreach (var transfer in transfers)
            {
                if (transfer.Lines == null) continue;

                foreach (var line in transfer.Lines)
                {
                    if (!string.IsNullOrEmpty(partnerId) && line.PartnerId?.ToString() != partnerId)
                        continue;

                    actionsList.Add(new PartnerActionDto
                    {
                        TransactionId = transfer.Id.ToString(),
                        TransactionCode = transfer.Code ?? "DOC",
                        TransactionType = "PartnerTransfer",
                        TransactionDate = transfer.Date,
                        ActionOfficeId = line.OfficeId?.ToString() ?? Guid.Empty.ToString(),
                        ActionPartnerId = line.PartnerId?.ToString() ?? string.Empty,
                        ActionDebit = line.DebitAmount,
                        ActionCredit = line.CreditAmount,
                        ActionEffect = line.DebitAmount - line.CreditAmount,
                        TransactionUserName = "admin",
                        TransactionIsCompleted = true,
                        TransactionIsDisabled = transfer.IsDisabled
                    });
                }
            }

            var sortedResult = actionsList
                .OrderByDescending(x => x.TransactionDate)
                .ToList();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            return Results.Json(sortedResult, jsonOptions);
        });
    }
    #region JSON Helpers
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

    private static string GetJsonString(JsonElement element, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (TryGetPropertyCaseInsensitive(element, name, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString();
        }
        return null;
    }

    private static decimal GetJsonDecimal(JsonElement element, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (TryGetPropertyCaseInsensitive(element, name, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number) return prop.GetDecimal();
                if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), out var val)) return val;
            }
        }
        return 0m;
    }
    #endregion

    public class PartnerActionDto
    {
        public string TransactionId { get; set; } = null!;
        public string TransactionCode { get; set; } = null!;
        public string TransactionType { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string ActionOfficeId { get; set; } = null!;
        public string ActionPartnerId { get; set; } = null!;
        public decimal ActionDebit { get; set; }
        public decimal ActionCredit { get; set; }
        public decimal ActionEffect { get; set; }
        public string TransactionUserName { get; set; } = null!;

        // --- ВАЖНЫЕ ПОЛЯ ДЛЯ ФИЛЬТРА ИНТЕРФЕЙСА ---
        public bool TransactionIsCompleted { get; set; }
        public bool TransactionIsDisabled { get; set; }
    }
}