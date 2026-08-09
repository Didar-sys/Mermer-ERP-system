using System;
using System.Linq;
using Mermer.Commerce.Models;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Abstractions;
using Microsoft.EntityFrameworkCore;

// Псевдонимы типов для исключения неоднозначности
using UIInvoice = Mermer.Commerce.Models.Invoice;
using PgInvoice = Mermer.Data.Postgres.Models.Invoice;
using PgInvoiceType = Mermer.Data.Postgres.Models.InvoiceType;

namespace Mermer.Api.Endpoints;

public static class InvoicesEndpoints
{
    public static IEndpointRouteBuilder MapInvoicesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").WithTags("Invoices");

        // --- 1. СПИСОК НАКЛАДНЫХ (С МАППИНГОМ ПОЛЕЙ ДЛЯ WPF-КЛИЕНТА) ---
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

            // Переводим серверные поля в точные названия клиентской модели WPF
            var uiResponse = info.Select(i => new
            {
                Id = i.Id,
                Code = i.Code,
                Type = i.InvoiceType.ToString(), // UI ждет строку
                Date = i.Date,
                UserId = i.UserId,
                UserName = i.UserName,
                IsCash = true,
                IsCompleted = i.IsCompleted,
                IsDisabled = i.IsDisabled, // <-- ЭТО ВЕРНЕТ КРАСНЫЙ ЦВЕТ!
                Group = i.Group,
                Tags = i.Tags,
                OfficeId = i.OfficeId,
                WarehouseId = i.WarehouseId,
                DepositoryId = i.DepositoryId,
                PartnerId = i.PartnerId,

                // ФИНАНСОВЫЕ СУММЫ
                ActionTotal = i.Subtotal,
                ActionDiscountsTotal = i.DiscountsTotal,
                ActionGrandTotal = i.GrandTotal
            });

            return Results.Ok(uiResponse);
        })
         .WithName("InvoicesGetInfo");

        // --- 2. КОЛИЧЕСТВО НАКЛАДНЫХ ---
        group.MapGet("/count", async (
            DateTime? from,
            DateTime? till,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var startDate = from ?? DateTime.MinValue;
            var endDate = till ?? DateTime.MaxValue;

            var count = await repo.CountInfoAsync(startDate, endDate, ct);
            return Results.Ok(new { count });
        })
        .WithName("InvoicesCountInfo");

        // --- 3. НАКЛАДНАЯ ПО ID ---
        group.MapGet("/{id}", async (
            string id,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var inv = await repo.GetAsync(id, ct);
            return inv is null ? Results.NotFound() : Results.Ok(inv);
        })
        .WithName("InvoicesGetById");

        // --- 4. АВТОНУМЕРАТОР НАКЛАДНЫХ ---
        group.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.Invoices.CountAsync();
            var nextCode = $"INV-{(count + 1):D6}";
            return Results.Ok(new { code = nextCode });
        })
        .WithName("InvoicesGetNextCode");

        // --- 5. СОЗДАНИЕ (POST) ---
        group.MapPost("/", async (
            UIInvoice model,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var pgModel = MapToPgInvoice(model);
            var created = await repo.CreateAsync(pgModel, ct);
            return Results.Created($"/api/invoices/{created.Id}", created);
        })
        .WithName("InvoicesCreate");

        // --- 6. ОБНОВЛЕНИЕ (PUT) ---
        group.MapPut("/{id}", async (
            string id,
            UIInvoice model,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            model.Id = id;
            var pgModel = MapToPgInvoice(model);
            var updated = await repo.UpdateAsync(pgModel, ct);
            return Results.Ok(updated);
        })
        .WithName("InvoicesUpdate");

        // --- 7. УДАЛЕНИЕ (DELETE) ---
        group.MapDelete("/{id}", async (
            string id,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            await repo.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("InvoicesDelete");

        return app;
    }

    private static PgInvoice MapToPgInvoice(UIInvoice src)
    {
        Enum.TryParse<PgInvoiceType>(src.InvoiceType.ToString(), out var parsedType);

        return new PgInvoice
        {
            Id = src.Id,
            Code = src.Code,
            Date = src.Date,
            DueDate = src.DueDate,
            InvoiceType = parsedType,
            UserId = src.UserId,
            UserName = src.UserName,
            OfficeId = src.OfficeId,
            WarehouseId = src.WarehouseId,
            DepositoryId = src.DepositoryId,
            PartnerId = src.PartnerId,
            DisplayCurrencyId = src.DisplayCurrencyId,
            StockPriceGroup = src.StockPriceGroup,
            IsCompleted = src.IsCompleted,
            IsDisabled = src.IsDisabled,
            Description = src.Description,
            Group = src.Group,
            Tags = src.Tags?.ToList()
        };
    }
}