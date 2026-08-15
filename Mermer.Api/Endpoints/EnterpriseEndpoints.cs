using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;

namespace Mermer.Api.Endpoints;

public static class EnterpriseEndpoints
{
    public static void MapEnterpriseEndpoints(this IEndpointRouteBuilder routes)
    {
        // Опции для строгого сохранения регистра PascalCase для WPF-клиента
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };

        // 1. Офисы (КРИТИЧНО: WPF-клиент запрашивает /api/enterprise/offices)
        routes.MapGet("/api/enterprise/offices", async (MermerDbContext db) =>
        {
            var offices = await db.Offices.AsNoTracking().Where(o => !o.IsDisabled).ToListAsync();
            var result = offices.Select(o => new
            {
                Id = o.Id.ToString(),
                Name = o.Name,
                IsDisabled = o.IsDisabled
            });
            return Results.Json(result, jsonOptions);
        }).WithTags("Enterprise");

        // 2. Склады (КРИТИЧНО: WPF-клиент запрашивает /api/enterprise/warehouses)
        // Возвращаем OfficeId для работы фильтра во View!
        routes.MapGet("/api/enterprise/warehouses", async (MermerDbContext db) =>
        {
            var warehouses = await db.Warehouses.AsNoTracking().Where(w => !w.IsDisabled).ToListAsync();
            var result = warehouses.Select(w => new
            {
                Id = w.Id.ToString(),
                Name = w.Name,
                OfficeId = w.OfficeId?.ToString(),
                IsDisabled = w.IsDisabled
            });
            return Results.Json(result, jsonOptions);
        }).WithTags("Enterprise");
    }
}