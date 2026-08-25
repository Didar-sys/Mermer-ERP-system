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

public static class PartnersEndpoints
{
    public static void MapPartnersEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/partners").WithTags("Partners");

        // 1. СПИСОК ПАРТНЕРОВ
        group.MapGet("/", async (MermerDbContext db) =>
        {
            var partners = await db.Partners.AsNoTracking().Where(p => !p.IsDisabled).ToListAsync();
            return Results.Ok(partners);
        });

        // 2. ФАСЕТЫ ДЛЯ ПАРТНЕРОВ
        group.MapGet("/facets", async (string? fields, MermerDbContext db, CancellationToken ct) =>
        {
            var fieldList = fields?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(f => f.Trim())
                                  .ToArray() ?? Array.Empty<string>();

            var result = new Dictionary<string, Dictionary<string, int>>();

            foreach (var field in fieldList)
            {
                if (field.Equals("Group", StringComparison.OrdinalIgnoreCase) || field.Equals("GroupNames", StringComparison.OrdinalIgnoreCase))
                {
                    var groups = await db.Partners
                        .AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Group))
                        .GroupBy(x => x.Group!)
                        .Select(g => new { Key = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

                    result[field] = groups;
                }
                else if (field.Equals("Tags", StringComparison.OrdinalIgnoreCase) || field.Equals("TagNames", StringComparison.OrdinalIgnoreCase))
                {
                    var allTags = await db.Partners
                        .AsNoTracking()
                        .Where(x => x.Tags != null && x.Tags.Length > 0)
                        .Select(x => x.Tags)
                        .ToListAsync(ct);

                    var tagCounts = allTags
                        .SelectMany(t => t!)
                        .GroupBy(t => t)
                        .ToDictionary(g => g.Key, g => g.Count());

                    result[field] = tagCounts;
                }
                else
                {
                    result[field] = new Dictionary<string, int>();
                }
            }

            return Results.Ok(result);
        })
        .WithName("PartnersGetFacets");

        group.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.Partners.CountAsync();
            var nextCode = $"P-{(count + 1):D5}";
            return Results.Ok(new { code = nextCode });
        });

        // 3. РАСЧЕТ БАЛАНСОВ ПАРТНЕРОВ
        group.MapGet("/balances/by-type", async (string? partnerId, DateTime? from, DateTime? till, [Microsoft.AspNetCore.Mvc.FromQuery] string[]? officeIds, MermerDbContext db) =>
        {
            var partnersQuery = db.Partners.AsNoTracking().Where(p => !p.IsDisabled);
            if (!string.IsNullOrEmpty(partnerId) && Guid.TryParse(partnerId, out var pGuid))
            {
                partnersQuery = partnersQuery.Where(p => p.Id == pGuid);
            }
            var partners = await partnersQuery.ToListAsync();

            var targetOfficeGuids = officeIds?
                .Select(x => Guid.TryParse(x, out var g) ? (Guid?)g : null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList() ?? new List<Guid>();

            var resultList = new List<object>();

            foreach (var partner in partners)
            {
                // Фильтр накладных с учетом офиса
                var invoicesQuery = db.Invoices.Include(i => i.Lines).AsNoTracking().Where(i => i.PartnerId == partner.Id);
                if (from.HasValue) invoicesQuery = invoicesQuery.Where(i => i.Date >= from.Value.ToUniversalTime());
                if (till.HasValue) invoicesQuery = invoicesQuery.Where(i => i.Date <= till.Value.ToUniversalTime());
                if (targetOfficeGuids.Any()) invoicesQuery = invoicesQuery.Where(i => i.OfficeId.HasValue && targetOfficeGuids.Contains(i.OfficeId.Value));

                var invoices = await invoicesQuery.ToListAsync();

                decimal sales = invoices.Where(i => i.InvoiceType == "Sales").Sum(i => i.Lines?.Sum(l => l.Quantity * l.Price) ?? 0m);
                decimal purchases = invoices.Where(i => i.InvoiceType == "Purchase").Sum(i => i.Lines?.Sum(l => l.Quantity * l.Price) ?? 0m);
                decimal salesReturn = invoices.Where(i => i.InvoiceType == "SalesReturn").Sum(i => i.Lines?.Sum(l => l.Quantity * l.Price) ?? 0m);
                decimal purchaseReturn = invoices.Where(i => i.InvoiceType == "PurchaseReturn").Sum(i => i.Lines?.Sum(l => l.Quantity * l.Price) ?? 0m);

                // Фильтр актов взаиморасчетов с учетом офиса
                var slipsQuery = db.PartnerSlips.Include(s => s.Lines).AsNoTracking();
                if (from.HasValue) slipsQuery = slipsQuery.Where(s => s.Date >= from.Value.ToUniversalTime());
                if (till.HasValue) slipsQuery = slipsQuery.Where(s => s.Date <= till.Value.ToUniversalTime());
                if (targetOfficeGuids.Any()) slipsQuery = slipsQuery.Where(s => s.OfficeId.HasValue && targetOfficeGuids.Contains(s.OfficeId.Value));

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

                decimal resultingBalance = opening + revision + sales - salesReturn - purchases + purchaseReturn;

                resultList.Add(new
                {
                    PartnerId = partner.Id.ToString(),
                    OfficeId = targetOfficeGuids.FirstOrDefault().ToString(),
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

        // 4. СОХРАНЕНИЕ ПАРТНЕРА (POST / PUT)
        Func<HttpRequest, MermerDbContext, Task<IResult>> savePartnerHandler = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = GetJsonString(root, "id", "Id");
            Guid partnerId = Guid.TryParse(idStr, out var parsedGuid) ? parsedGuid : Guid.NewGuid();

            string code = GetJsonString(root, "code", "Code") ?? $"P-{DateTime.UtcNow:yyMMddHHmmss}";
            string name = GetJsonString(root, "name", "Name") ?? "Новый партнер";
            string phone = GetJsonString(root, "phone", "Phone") ?? "";
            string address = GetJsonString(root, "address", "Address") ?? "";
            string groupName = GetJsonString(root, "group", "Group") ?? "";

            var tagsList = ExtractTagsFromRawJson(root);

            var existing = await db.Partners.FirstOrDefaultAsync(p => p.Id == partnerId);
            if (existing == null)
            {
                var entity = new PartnerEntity
                {
                    Id = partnerId,
                    Code = code,
                    Name = name,
                    Phone = phone,
                    Address = address,
                    Group = groupName,
                    Tags = tagsList.ToArray(),
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
                existing.Address = address;
                existing.Group = groupName;
                existing.Tags = tagsList.ToArray();
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.Content($"{{\"id\":\"{partnerId}\",\"code\":\"{code}\"}}", "application/json");
        };

        group.MapPost("/", savePartnerHandler);
        group.MapPut("/{id}", savePartnerHandler);
        routes.MapPost("/api/catalog/partners", savePartnerHandler);

        // 5. ФАСЕТЫ ДЛЯ PARTNER SLIPS
        group.MapGet("/slips/facets", async (HttpContext context, MermerDbContext db, CancellationToken ct) =>
        {
            string? fields = context.Request.Query["fields"].ToString();
            var fieldList = string.IsNullOrEmpty(fields)
                ? new[] { "Date", "Group", "Tags" }
                : fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .ToArray();

            var result = new Dictionary<string, Dictionary<string, int>>();

            foreach (var field in fieldList)
            {
                if (field.Equals("Group", StringComparison.OrdinalIgnoreCase) || field.Equals("GroupNames", StringComparison.OrdinalIgnoreCase))
                {
                    var groups = await db.PartnerSlips
                        .AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Group))
                        .GroupBy(x => x.Group!)
                        .Select(g => new { Key = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

                    result[field] = groups;
                }
                else if (field.Equals("Tags", StringComparison.OrdinalIgnoreCase) || field.Equals("TagNames", StringComparison.OrdinalIgnoreCase))
                {
                    var allTags = await db.PartnerSlips
                        .AsNoTracking()
                        .Where(x => x.Tags != null && x.Tags.Length > 0)
                        .Select(x => x.Tags)
                        .ToListAsync(ct);

                    var tagCounts = allTags
                        .SelectMany(t => t!)
                        .GroupBy(t => t)
                        .ToDictionary(g => g.Key, g => g.Count());

                    result[field] = tagCounts;
                }
                else
                {
                    result[field] = new Dictionary<string, int>();
                }
            }

            return Results.Ok(result);
        })
        .WithName("PartnerSlipsGetFacets");

        // 6. ПОДГРУЗКА PARTNER SLIPS
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
                UserName = s.UserName ?? "admin",
                Group = s.Group ?? string.Empty,
                Tags = s.Tags != null ? s.Tags.ToList() : new List<string>(),
                Description = s.Description ?? string.Empty,
                IsDisabled = s.IsDisabled,
                IsCompleted = s.IsCompleted,
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
                PropertyNamingPolicy = null,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            return Results.Json(result, jsonOptions);
        });

        // 7. СОХРАНЕНИЕ PARTNER SLIPS
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

            string groupName = GetJsonString(root, "group", "Group") ?? string.Empty;
            string description = GetJsonString(root, "description", "Description") ?? string.Empty;
            string userName = GetJsonString(root, "userName", "UserName") ?? "admin";
            var tagsList = ExtractTagsFromRawJson(root);

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

            if (existing == null)
            {
                var entity = new PartnerSlipEntity
                {
                    Id = slipId,
                    Code = code,
                    Date = slipDate,
                    SlipType = slipType,
                    OfficeId = officeGuid,
                    UserName = userName,
                    Group = groupName,
                    Tags = tagsList.ToArray(),
                    Description = description,
                    IsCompleted = true,
                    IsDisabled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await db.PartnerSlips.AddAsync(entity);
            }
            else
            {
                existing.Code = code;
                existing.Date = slipDate;
                existing.SlipType = slipType;
                existing.OfficeId = officeGuid;
                existing.UserName = userName;
                existing.Group = groupName;
                existing.Tags = tagsList.ToArray();
                existing.Description = description;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            await db.Database.ExecuteSqlRawAsync("DELETE FROM partner_slip_lines WHERE partner_slip_id = {0}", slipId);

            if (linesList.Any())
            {
                await db.PartnerSlipLines.AddRangeAsync(linesList);
                await db.SaveChangesAsync();
            }

            return Results.Content($"{{\"id\":\"{slipId}\",\"code\":\"{code}\"}}", "application/json");
        });

        // 8. ФАСЕТЫ ДЛЯ PARTNER TRANSFERS (GroupNames, TagNames)
        group.MapGet("/transfers/facets", async (HttpContext context, MermerDbContext db, CancellationToken ct) =>
        {
            string? fields = context.Request.Query["fields"].ToString();
            var fieldList = string.IsNullOrEmpty(fields)
                ? new[] { "Date", "Group", "Tags" }
                : fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .ToArray();

            var result = new Dictionary<string, Dictionary<string, int>>();

            foreach (var field in fieldList)
            {
                if (field.Equals("Group", StringComparison.OrdinalIgnoreCase) || field.Equals("GroupNames", StringComparison.OrdinalIgnoreCase))
                {
                    var groups = await db.PartnerTransfers
                        .AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Group))
                        .GroupBy(x => x.Group!)
                        .Select(g => new { Key = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

                    result[field] = groups;
                }
                else if (field.Equals("Tags", StringComparison.OrdinalIgnoreCase) || field.Equals("TagNames", StringComparison.OrdinalIgnoreCase))
                {
                    var allTags = await db.PartnerTransfers
                        .AsNoTracking()
                        .Where(x => x.Tags != null && x.Tags.Length > 0)
                        .Select(x => x.Tags)
                        .ToListAsync(ct);

                    var tagCounts = allTags
                        .SelectMany(t => t!)
                        .GroupBy(t => t)
                        .ToDictionary(g => g.Key, g => g.Count());

                    result[field] = tagCounts;
                }
                else
                {
                    result[field] = new Dictionary<string, int>();
                }
            }

            return Results.Ok(result);
        })
        .WithName("PartnerTransfersGetFacets");

        // 9. ПОДГРУЗКА ПЕРЕВОДОВ PARTNER TRANSFERS
        group.MapGet("/transfers", async (MermerDbContext db) =>
        {
            var transfers = await db.PartnerTransfers
                .Include(t => t.Lines)
                .AsNoTracking()
                .ToListAsync();

            var allRates = await db.CurrencyRates.AsNoTracking().ToListAsync();

            var result = transfers.Select(t =>
            {
                var usedCurrencyIds = t.Lines != null
                    ? t.Lines.Select(l => l.DebitCurrencyId)
                             .Union(t.Lines.Select(l => l.CreditCurrencyId))
                             .Where(c => c.HasValue)
                             .Select(c => c!.Value)
                             .Distinct()
                             .ToList()
                    : new List<Guid>();

                var currencyConvertions = usedCurrencyIds.Select(cId =>
                {
                    var rate = allRates
                        .Where(r => r.CurrencyId == cId && r.ValidFrom <= t.Date)
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
                    UserName = t.UserName ?? "admin",
                    Group = t.Group ?? string.Empty,
                    Tags = t.Tags != null ? t.Tags.ToList() : new List<string>(),
                    Description = t.Description ?? string.Empty,
                    IsDisabled = t.IsDisabled,
                    IsCompleted = t.IsCompleted,
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

        // 10. СОХРАНЕНИЕ ПЕРЕВОДОВ PARTNER TRANSFERS
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

            string groupName = GetJsonString(root, "group", "Group") ?? string.Empty;
            string description = GetJsonString(root, "description", "Description") ?? string.Empty;
            string userName = GetJsonString(root, "userName", "UserName") ?? "admin";
            var tagsList = ExtractTagsFromRawJson(root);

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
                    UserName = userName,
                    Group = groupName,
                    Tags = tagsList.ToArray(),
                    Description = description,
                    IsCompleted = true,
                    IsDisabled = false,
                    Lines = linesList,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await db.PartnerTransfers.AddAsync(entity);
            }
            else
            {
                existing.Code = code;
                existing.Date = transferDate;
                existing.UserName = userName;
                existing.Group = groupName;
                existing.Tags = tagsList.ToArray();
                existing.Description = description;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            await db.Database.ExecuteSqlRawAsync("DELETE FROM partner_transfer_lines WHERE partner_transfer_id = {0}", transferId);

            if (linesList.Any())
            {
                await db.PartnerTransferLines.AddRangeAsync(linesList);
                await db.SaveChangesAsync();
            }

            return Results.Content($"{{\"id\":\"{transferId}\",\"code\":\"{code}\"}}", "application/json");
        });

        // 11. РЕЕСТР ДВИЖЕНИЙ ПО ПАРТНЕРАМ (PARTNERACTIONS)
        group.MapGet("/actions", async (string? partnerId, DateTime? from, DateTime? till, string[]? officeIds, MermerDbContext db) =>
        {
            var actionsList = new List<PartnerActionDto>();

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
                    TransactionDate = inv.Date.DateTime,
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
                        TransactionDate = slip.Date,
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
        public bool TransactionIsCompleted { get; set; }
        public bool TransactionIsDisabled { get; set; }
    }
}