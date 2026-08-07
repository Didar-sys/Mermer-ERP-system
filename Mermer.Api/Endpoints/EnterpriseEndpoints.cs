using Mermer.Api.DTOs;
using Mermer.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace Mermer.Api.Endpoints;

public static class EnterpriseEndpoints
{
    public static void MapEnterpriseEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/enterprise").WithTags("Enterprise");

        // --- СКЛАДЫ (WAREHOUSES) ---

        group.MapGet("/warehouses", async (MermerDbContext db) =>
        {
            var warehouses = await db.Warehouses
                .AsNoTracking()
                .Select(w => new WarehouseDetailsDto(
                    w.Id.ToString(),
                    w.Name,
                    w.OfficeId.HasValue ? w.OfficeId.Value.ToString() : null,
                    w.Description
                ))
                .ToListAsync();

            return Results.Ok(warehouses);
        })
        .WithName("GetWarehouses")
        .WithSummary("Получить список всех складов");

        group.MapGet("/warehouses/{id}", async (string id, MermerDbContext db) =>
        {
            if (!Guid.TryParse(id, out var guidId))
                return Results.BadRequest(new { message = "Некорректный формат Id." });

            var warehouse = await db.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == guidId);

            if (warehouse == null)
                return Results.NotFound();

            return Results.Ok(new WarehouseDetailsDto(
                warehouse.Id.ToString(),
                warehouse.Name,
                warehouse.OfficeId.HasValue ? warehouse.OfficeId.Value.ToString() : null,
                warehouse.Description
            ));
        })
        .WithName("GetWarehouseById")
        .WithSummary("Получить склад по Id");

        // --- ОФИСЫ (OFFICES) ---

        group.MapGet("/offices", async (MermerDbContext db) =>
        {
            var offices = await db.Offices
                .AsNoTracking()
                .Select(o => new OfficeDto(
                    o.Id.ToString(),
                    o.Name,
                    o.Description
                ))
                .ToListAsync();

            return Results.Ok(offices);
        })
        .WithName("GetOffices")
        .WithSummary("Получить список всех офисов");

        /// --- ВАЛЮТЫ (CURRENCIES) ---
        group.MapGet("/currencies", async (MermerDbContext db) =>
        {
            var currencies = await db.Currencies
                .AsNoTracking()
                .Select(c => new CurrencyDto(
                    c.Id.ToString(),
                    c.Name
                ))
                .ToListAsync();

            return Results.Ok(currencies);
        })
        .WithName("GetCurrencies")
        .WithSummary("Получить список всех валют");
    }
}