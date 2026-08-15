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

namespace Mermer.Api.Endpoints;

public static class StockTransfersEndpoints
{
    public static void MapStockTransfersEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/warehousing/transfers").WithTags("StockTransfers");

        group.MapGet("/", async (MermerDbContext db) =>
        {
            // Возвращаем список перемещений (заглушка/чтение)
            return Results.Ok(new object[] { });
        });

        Func<HttpRequest, MermerDbContext, Task<IResult>> saveTransferHandler = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp) ? idProp.GetString() : null;
            Guid transferId = Guid.TryParse(idStr, out var parsedGuid) ? parsedGuid : Guid.NewGuid();

            // В Postgres сохранение перемещений при необходимости
            await db.SaveChangesAsync();

            return Results.Ok(new { id = transferId });
        };

        group.MapPost("/", saveTransferHandler);
        group.MapPut("/{id}", saveTransferHandler);
    }
}