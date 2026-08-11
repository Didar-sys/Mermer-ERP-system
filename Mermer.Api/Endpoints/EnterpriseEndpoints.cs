using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Mermer.Api.DTOs;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Entities;

namespace Mermer.Api.Endpoints;

public static class EnterpriseEndpoints
{
    public static void MapEnterpriseEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/enterprise").WithTags("Enterprise");

        // ================= WAREHOUSES =================
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

        Func<HttpRequest, MermerDbContext, Task<IResult>> saveWarehouse = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp) ? idProp.GetString() : null;
            Guid id = Guid.TryParse(idStr, out var parsed) ? parsed : Guid.NewGuid();

            var existing = await db.Warehouses.FirstOrDefaultAsync(x => x.Id == id);
            string name = root.TryGetProperty("name", out var n) || root.TryGetProperty("Name", out n) ? n.GetString() : "Новый склад";
            string desc = root.TryGetProperty("description", out var d) || root.TryGetProperty("Description", out d) ? d.GetString() : "";

            if (existing == null)
            {
                await db.Warehouses.AddAsync(new WarehouseEntity { Id = id, Name = name, Description = desc });
            }
            else
            {
                existing.Name = name;
                existing.Description = desc;
            }
            await db.SaveChangesAsync();
            return Results.Ok(new { id });
        };

        group.MapPost("/warehouses", saveWarehouse);
        group.MapPut("/warehouses/{id}", saveWarehouse);


        // ================= OFFICES =================
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

        Func<HttpRequest, MermerDbContext, Task<IResult>> saveOffice = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp) ? idProp.GetString() : null;
            Guid id = Guid.TryParse(idStr, out var parsed) ? parsed : Guid.NewGuid();

            var existing = await db.Offices.FirstOrDefaultAsync(x => x.Id == id);
            string name = root.TryGetProperty("name", out var n) || root.TryGetProperty("Name", out n) ? n.GetString() : "Новый офис";
            string desc = root.TryGetProperty("description", out var d) || root.TryGetProperty("Description", out d) ? d.GetString() : "";

            if (existing == null)
            {
                await db.Offices.AddAsync(new OfficeEntity { Id = id, Name = name, Description = desc });
            }
            else
            {
                existing.Name = name;
                existing.Description = desc;
            }
            await db.SaveChangesAsync();
            return Results.Ok(new { id });
        };

        group.MapPost("/offices", saveOffice);
        group.MapPut("/offices/{id}", saveOffice);


        // ================= CURRENCIES =================
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

        Func<HttpRequest, MermerDbContext, Task<IResult>> saveCurrency = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp) ? idProp.GetString() : null;
            Guid id = Guid.TryParse(idStr, out var parsed) ? parsed : Guid.NewGuid();

            var existing = await db.Currencies.FirstOrDefaultAsync(x => x.Id == id);
            string name = root.TryGetProperty("name", out var n) || root.TryGetProperty("Name", out n) ? n.GetString() : "USD";

            if (existing == null)
            {
                await db.Currencies.AddAsync(new CurrencyEntity { Id = id, Name = name });
            }
            else
            {
                existing.Name = name;
            }
            await db.SaveChangesAsync();
            return Results.Ok(new { id });
        };

        group.MapPost("/currencies", saveCurrency);
        group.MapPut("/currencies/{id}", saveCurrency);
    }
}