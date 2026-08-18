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

public static class StockRevisionsEndpoints
{
    public static IEndpointRouteBuilder MapStockRevisionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warehousing/revisions").WithTags("StockRevisions");

        JsonElement GetProp(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var val)) return val;
            if (el.TryGetProperty(char.ToLower(name[0]) + name.Substring(1), out val)) return val;
            return default;
        }

        group.MapGet("/", async (MermerDbContext db, CancellationToken ct) =>
        {
            var list = await db.StockRevisions.AsNoTracking().OrderByDescending(r => r.Date).ToListAsync(ct);
            return Results.Ok(list.Select(r => new
            {
                Id = r.Id.ToString(),
                Code = r.Code ?? "",
                Date = r.Date.UtcDateTime,
                FinishDate = r.FinishDate?.UtcDateTime,
                WarehouseId = r.WarehouseId?.ToString(),
                ExceedSlipId = r.ExceedSlipId?.ToString(),
                DeficitSlipId = r.DeficitSlipId?.ToString(),
                UserId = r.UserId?.ToString(),
                UserName = r.UserName ?? "",
                IsCompleted = r.IsCompleted,
                IsDisabled = r.IsDisabled,
                Group = r.GroupName ?? "",
                Tags = r.Tags ?? Array.Empty<string>(),
                Description = r.Description ?? ""
            }));
        });

        group.MapGet("/{id}", async (string id, MermerDbContext db, CancellationToken ct) =>
        {
            if (!Guid.TryParse(id, out var guid)) return Results.NotFound();
            var r = await db.StockRevisions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == guid, ct);
            if (r == null) return Results.NotFound();

            return Results.Ok(new
            {
                Id = r.Id.ToString(),
                Code = r.Code ?? "",
                Date = r.Date.UtcDateTime,
                FinishDate = r.FinishDate?.UtcDateTime,
                WarehouseId = r.WarehouseId?.ToString(),
                ExceedSlipId = r.ExceedSlipId?.ToString(),
                DeficitSlipId = r.DeficitSlipId?.ToString(),
                UserId = r.UserId?.ToString(),
                UserName = r.UserName ?? "",
                IsCompleted = r.IsCompleted,
                IsDisabled = r.IsDisabled,
                Group = r.GroupName ?? "",
                Tags = r.Tags ?? Array.Empty<string>(),
                Description = r.Description ?? ""
            });
        });

        group.MapGet("/{id}/lines", async (string id, MermerDbContext db, CancellationToken ct) =>
        {
            if (!Guid.TryParse(id, out var guid)) return Results.Ok(new object[0]);
            var lines = await db.StockRevisionLines.AsNoTracking().Where(l => l.StockRevisionId == guid).ToListAsync(ct);

            return Results.Ok(lines.Select(l => new
            {
                Id = l.Id.ToString(),
                StockRevisionId = l.StockRevisionId.ToString(),
                StockId = l.StockId?.ToString(),
                Date = l.Date.UtcDateTime,
                Quantity = l.Quantity,
                UnitId = l.UnitId?.ToString(),
                Price = l.Price,
                CurrencyId = l.CurrencyId?.ToString(),
                UserId = l.UserId?.ToString(),
                UserName = l.UserName ?? ""
            }));
        });

        group.MapPost("/{id}/lines", async (string id, HttpRequest req, MermerDbContext db) =>
        {
            if (!Guid.TryParse(id, out var revGuid)) return Results.BadRequest("Invalid Revision ID");
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var revision = await db.StockRevisions.FirstOrDefaultAsync(r => r.Id == revGuid);
            if (revision == null)
            {
                revision = new StockRevisionEntity { Id = revGuid, Code = $"REV-{DateTime.UtcNow:yyMMddHHmmss}", Date = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
                await db.StockRevisions.AddAsync(revision);
            }

            var ip = GetProp(root, "Id");
            Guid lineId = ip.ValueKind != JsonValueKind.Undefined && Guid.TryParse(ip.GetString(), out var parsedLineId) && parsedLineId != Guid.Empty ? parsedLineId : Guid.NewGuid();

            var line = await db.StockRevisionLines.FirstOrDefaultAsync(l => l.Id == lineId);
            if (line == null)
            {
                line = new StockRevisionLineEntity { Id = lineId, StockRevisionId = revGuid };
                await db.StockRevisionLines.AddAsync(line);
            }

            var sp = GetProp(root, "StockId");
            if (sp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(sp.GetString(), out var sG)) line.StockId = sG;

            var up = GetProp(root, "UnitId");
            if (up.ValueKind != JsonValueKind.Undefined && Guid.TryParse(up.GetString(), out var uG)) line.UnitId = uG;

            var qp = GetProp(root, "Quantity");
            if (qp.ValueKind == JsonValueKind.Number) line.Quantity = qp.GetDecimal();
            else if (qp.ValueKind == JsonValueKind.String && decimal.TryParse(qp.GetString(), out var qD)) line.Quantity = qD;

            var pp = GetProp(root, "Price");
            if (pp.ValueKind == JsonValueKind.Number) line.Price = pp.GetDecimal();
            else if (pp.ValueKind == JsonValueKind.String && decimal.TryParse(pp.GetString(), out var pD)) line.Price = pD;

            var cp = GetProp(root, "CurrencyId");
            if (cp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(cp.GetString(), out var cG)) line.CurrencyId = cG;

            var usrp = GetProp(root, "UserId");
            if (usrp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(usrp.GetString(), out var usrG)) line.UserId = usrG;

            var unp = GetProp(root, "UserName");
            if (unp.ValueKind != JsonValueKind.Undefined) line.UserName = unp.GetString();

            line.Date = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(new { id = lineId });
        });

        group.MapDelete("/lines/{lineId}", async (string lineId, MermerDbContext db) =>
        {
            if (Guid.TryParse(lineId, out var g))
            {
                var l = await db.StockRevisionLines.FirstOrDefaultAsync(x => x.Id == g);
                if (l != null)
                {
                    db.StockRevisionLines.Remove(l);
                    await db.SaveChangesAsync();
                }
            }
            return Results.Ok();
        });

        group.MapPost("/", async (HttpRequest req, MermerDbContext db) =>
        {
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var ip = GetProp(root, "Id");
            Guid revId = ip.ValueKind != JsonValueKind.Undefined && Guid.TryParse(ip.GetString(), out var parsedGuid) && parsedGuid != Guid.Empty ? parsedGuid : Guid.NewGuid();

            var existing = await db.StockRevisions.FirstOrDefaultAsync(r => r.Id == revId);
            if (existing == null)
            {
                existing = new StockRevisionEntity { Id = revId, CreatedAt = DateTimeOffset.UtcNow };
                await db.StockRevisions.AddAsync(existing);
            }

            var cp = GetProp(root, "Code");
            if (cp.ValueKind != JsonValueKind.Undefined) existing.Code = cp.GetString();

            var wp = GetProp(root, "WarehouseId");
            if (wp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(wp.GetString(), out var wG)) existing.WarehouseId = wG;

            var comp = GetProp(root, "IsCompleted");
            if (comp.ValueKind == JsonValueKind.True || comp.ValueKind == JsonValueKind.False) existing.IsCompleted = comp.GetBoolean();

            var dis = GetProp(root, "IsDisabled");
            if (dis.ValueKind == JsonValueKind.True || dis.ValueKind == JsonValueKind.False) existing.IsDisabled = dis.GetBoolean();

            var gp = GetProp(root, "Group"); // ИСПРАВЛЕНИЕ: передан root
            if (gp.ValueKind != JsonValueKind.Undefined) existing.GroupName = gp.GetString();

            var dp = GetProp(root, "Description"); // ИСПРАВЛЕНИЕ: передан root
            if (dp.ValueKind != JsonValueKind.Undefined) existing.Description = dp.GetString();

            var fdp = GetProp(root, "FinishDate");
            if (fdp.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(fdp.GetString(), out var fDate))
                existing.FinishDate = fDate.ToUniversalTime();

            existing.Date = DateTimeOffset.UtcNow;
            existing.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(new { id = revId, code = existing.Code });
        });

        return app;
    }
}