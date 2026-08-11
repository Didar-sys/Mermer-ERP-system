using System;
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

        // Общая логика обработки сохранения партнера
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

        group.MapGet("/balances", async (MermerDbContext db) =>
        {
            // Здесь будет агрегация данных из транзакций и счетов. 
            // Пока возвращаем пустой массив, чтобы WPF-клиент не крашился при открытии вкладки "Взаиморасчеты".
            return Results.Ok(new object[] { });
        });

        // Регистрируем обработчик для обоих путей
        group.MapPost("/", savePartnerHandler);
        routes.MapPost("/api/catalog/partners", savePartnerHandler);
    }
}