using System;
using System.Linq;
using Mermer.CRM.Models;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mermer.Api.Endpoints;

public static class PartnersEndpoints
{
    public static IEndpointRouteBuilder MapPartnersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalog/partners").WithTags("Partners");

        // --- 1. ЧТЕНИЕ СПИСКА ПАРТНЕРОВ (СО ВСЕМИ ПОЛЯМИ) ---
        group.MapGet("", async (MermerDbContext db) =>
        {
            var partners = await db.Partners
                .AsNoTracking()
                .Select(p => new Partner
                {
                    Id = p.Id.ToString(),
                    Code = p.Code ?? string.Empty,
                    Name = p.Name ?? string.Empty,
                    Phone = p.Phone ?? string.Empty,
                    Address = p.Address ?? string.Empty,
                    CreditLimit = p.CreditLimit,
                    IsDisabled = p.IsDisabled
                })
                .ToListAsync();

            return Results.Ok(partners);
        })
        .WithName("GetPartnersList");

        // --- 2. ЧТЕНИЕ ПО ID ---
        group.MapGet("/{id}", async (string id, MermerDbContext db) =>
        {
            if (!Guid.TryParse(id, out var guidId)) return Results.BadRequest("Invalid Guid");

            var entity = await db.Partners.AsNoTracking().FirstOrDefaultAsync(p => p.Id == guidId);
            if (entity == null) return Results.NotFound();

            var model = new Partner
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                Code = entity.Code,
                Phone = entity.Phone,
                Address = entity.Address,
                CreditLimit = entity.CreditLimit,
                IsDisabled = entity.IsDisabled
            };

            return Results.Ok(model);
        });

        // --- 3. СОЗДАНИЕ (POST) ---
        group.MapPost("", async (Partner model, MermerDbContext db) =>
        {
            Guid partnerGuid = Guid.TryParse(model.Id, out var parsed) ? parsed : Guid.NewGuid();

            var entity = new PartnerEntity
            {
                Id = partnerGuid,
                Name = model.Name ?? string.Empty,
                Code = model.Code,
                Phone = model.Phone,
                Address = model.Address,
                CreditLimit = model.CreditLimit,
                IsDisabled = model.IsDisabled,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.Partners.Add(entity);
            await db.SaveChangesAsync();

            model.Id = entity.Id.ToString();
            return Results.Created($"/api/catalog/partners/{model.Id}", model);
        });

        // --- 4. ОБНОВЛЕНИЕ (PUT) ---
        group.MapPut("/{id}", async (string id, Partner model, MermerDbContext db) =>
        {
            if (!Guid.TryParse(id, out var guidId)) return Results.BadRequest("Invalid Guid");

            var entity = await db.Partners.FindAsync(guidId);
            if (entity == null) return Results.NotFound();

            entity.Name = model.Name ?? entity.Name;
            entity.Code = model.Code;
            entity.Phone = model.Phone;
            entity.Address = model.Address;
            entity.CreditLimit = model.CreditLimit;
            entity.IsDisabled = model.IsDisabled;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(model);
        });

        // --- 5. УДАЛЕНИЕ (DELETE) ---
        group.MapDelete("/{id}", async (string id, MermerDbContext db) =>
        {
            if (!Guid.TryParse(id, out var guidId)) return Results.BadRequest("Invalid Guid");

            var entity = await db.Partners.FindAsync(guidId);
            if (entity != null)
            {
                db.Partners.Remove(entity);
                await db.SaveChangesAsync();
            }

            return Results.NoContent();
        });

        // --- 6. БАЛАНС ПАРТНЕРА ---
        group.MapGet("/{id}/balance", async (string id, string? officeId, DateTime? date, MermerDbContext db) =>
        {
            return Results.Ok(new { Balance = 0m });
        });

        // --- 7. АВТОНУМЕРАТОР ---
        group.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.Partners.CountAsync();
            var nextCode = $"P-{(count + 1):D5}";
            return Results.Ok(new { code = nextCode });
        })
        .WithName("PartnersGetNextCode");

        return app;
    }
}