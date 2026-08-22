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
using Mermer.Data.Postgres.Abstractions;

// Псевдоним только для серверной модели PostgreSQL
using PgInvoice = Mermer.Data.Postgres.Models.Invoice;

namespace Mermer.Api.Endpoints;

public static class InvoicesEndpoints
{
    public static IEndpointRouteBuilder MapInvoicesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").WithTags("Invoices");

        // --- 1. СПИСОК НАКЛАДНЫХ ---
        group.MapGet("/", async (
             DateTime? from,
             DateTime? till,
             string? displayCurrencyId,
             IInvoicesRepository repo,
             CancellationToken ct) =>
        {
            var startDate = from ?? DateTime.MinValue;
            var endDate = till ?? DateTime.MaxValue;

            var info = await repo.GetInfoAsync(startDate, endDate, displayCurrencyId, ct);

            var uiResponse = info.Select(i => new
            {
                Id = i.Id,
                Code = i.Code,
                Type = i.InvoiceType.ToString(),
                Date = i.Date,
                UserId = i.UserId,
                UserName = i.UserName,
                IsCash = true,
                IsCompleted = i.IsCompleted,
                IsDisabled = i.IsDisabled,
                Group = i.Group,
                Tags = i.Tags,
                OfficeId = i.OfficeId,
                WarehouseId = i.WarehouseId,
                DepositoryId = i.DepositoryId,
                PartnerId = i.PartnerId,
                ActionTotal = i.Subtotal,
                ActionDiscountsTotal = i.DiscountsTotal,
                ActionGrandTotal = i.GrandTotal
            });

            return Results.Ok(uiResponse);
        })
        .WithName("InvoicesGetInfo");

        // --- 2. КОЛИЧЕСТВО НАКЛАДНЫХ ---
        group.MapGet("/count", async (DateTime? from, DateTime? till, IInvoicesRepository repo, CancellationToken ct) =>
        {
            var startDate = from ?? DateTime.MinValue;
            var endDate = till ?? DateTime.MaxValue;
            var count = await repo.CountInfoAsync(startDate, endDate, ct);
            return Results.Ok(new { count });
        })
        .WithName("InvoicesCountInfo");

        // --- 3. ФАСЕТЫ (GroupNames, TagNames) ---
        group.MapGet("/facets", async (string? fields, MermerDbContext db, CancellationToken ct) =>
        {
            var fieldList = fields?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            ?? Array.Empty<string>();

            var result = new Dictionary<string, Dictionary<string, int>>();

            foreach (var field in fieldList)
            {
                if (field.Equals("Group", StringComparison.OrdinalIgnoreCase) || field.Equals("GroupNames", StringComparison.OrdinalIgnoreCase))
                {
                    var groups = await db.Invoices
                        .AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Group))
                        .GroupBy(x => x.Group!)
                        .Select(g => new { Key = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

                    result[field] = groups;
                }
                else if (field.Equals("Tags", StringComparison.OrdinalIgnoreCase) || field.Equals("TagNames", StringComparison.OrdinalIgnoreCase))
                {
                    var allTags = await db.Invoices
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
        .WithName("InvoicesGetFacets");

        // --- 4. НАКЛАДНАЯ ПО ID ---
        group.MapGet("/{id}", async (string id, IInvoicesRepository repo, CancellationToken ct) =>
        {
            var inv = await repo.GetAsync(id, ct);
            return inv is null ? Results.NotFound() : Results.Ok(inv);
        })
        .WithName("InvoicesGetById");

        // --- 5. АВТОНУМЕРАТОР НАКЛАДНЫХ ---
        group.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.Invoices.CountAsync();
            var nextCode = $"INV-{(count + 1):D6}";
            return Results.Ok(new { code = nextCode });
        })
        .WithName("InvoicesGetNextCode");

        // --- 6. СОЗДАНИЕ И ОБНОВЛЕНИЕ (POST / PUT) ЧЕРЕЗ JSON ---
        Func<HttpRequest, IInvoicesRepository, CancellationToken, Task<IResult>> saveInvoiceHandler = async (request, repo, ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var pgInvoice = JsonSerializer.Deserialize<PgInvoice>(body, options);

            if (pgInvoice == null) return Results.BadRequest("Invalid JSON");

            var existing = await repo.GetAsync(pgInvoice.Id, ct);

            try
            {
                if (existing == null)
                {
                    await repo.CreateAsync(pgInvoice, ct);
                    return Results.Created($"/api/invoices/{pgInvoice.Id}", pgInvoice);
                }
                else
                {
                    await repo.UpdateAsync(pgInvoice, ct);
                    return Results.Ok(pgInvoice);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Invoice Save Error]: {ex}");
                return Results.Problem(ex.Message);
            }
        };

        group.MapPost("/", saveInvoiceHandler).WithName("InvoicesCreate");
        group.MapPut("/{id}", async (string id, HttpRequest request, IInvoicesRepository repo, CancellationToken ct) => await saveInvoiceHandler(request, repo, ct)).WithName("InvoicesUpdate");

        // --- 7. УДАЛЕНИЕ (DELETE) ---
        group.MapDelete("/{id}", async (string id, IInvoicesRepository repo, CancellationToken ct) =>
        {
            await repo.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("InvoicesDelete");

        return app;
    }
}