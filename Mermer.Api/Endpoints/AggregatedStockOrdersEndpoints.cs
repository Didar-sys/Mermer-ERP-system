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

public static class AggregatedStockOrdersEndpoints
{
    public static IEndpointRouteBuilder MapAggregatedStockOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warehousing/aggregated-orders").WithTags("AggregatedStockOrders");

        JsonElement GetProp(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var val)) return val;
            if (el.TryGetProperty(char.ToLower(name[0]) + name.Substring(1), out val)) return val;
            return default;
        }

        group.MapGet("/", async (MermerDbContext db, CancellationToken ct) =>
        {
            var list = await db.AggregatedStockOrders
                .Include(o => o.Lines)
                .AsSplitQuery()
                .AsNoTracking()
                .OrderByDescending(o => o.Date)
                .ToListAsync(ct);

            return Results.Ok(list.Select(o => new
            {
                Id = o.Id.ToString(),
                Code = o.Code ?? "",
                Date = o.Date.UtcDateTime,
                WarehouseId = o.WarehouseId?.ToString(),
                PartnerId = o.PartnerId?.ToString(),
                UserId = o.UserId?.ToString(),
                UserName = o.UserName ?? "",
                IsCompleted = o.IsCompleted,
                IsDisabled = o.IsDisabled,
                Group = o.GroupName ?? "",
                Tags = o.Tags ?? Array.Empty<string>(),
                Description = o.Description ?? "",
                Lines = o.Lines.Select(l => new
                {
                    Id = l.Id.ToString(),
                    StockId = l.StockId?.ToString(),
                    UnitId = l.UnitId?.ToString(),
                    Orders = JsonSerializer.Deserialize<Dictionary<string, decimal>>(l.OrdersJson) ?? new Dictionary<string, decimal>()
                })
            }));
        });

        group.MapGet("/facets", async (string? fields, MermerDbContext db, CancellationToken ct) =>
        {
            var dict = new Dictionary<string, Dictionary<string, int>>();
            var allGroups = await db.AggregatedStockOrders
                .Where(x => x.GroupName != null && x.GroupName != "")
                .GroupBy(x => x.GroupName!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            dict["GroupNames"] = allGroups.ToDictionary(x => x.Key, x => x.Count);
            dict["TagNames"] = new Dictionary<string, int>();
            return Results.Ok(dict);
        });

        group.MapPost("/", async (HttpRequest req, MermerDbContext db) =>
        {
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var ip = GetProp(root, "Id");
            Guid orderId = ip.ValueKind != JsonValueKind.Undefined && Guid.TryParse(ip.GetString(), out var parsedGuid) && parsedGuid != Guid.Empty ? parsedGuid : Guid.NewGuid();

            var existing = await db.AggregatedStockOrders.Include(o => o.Lines).AsSplitQuery().FirstOrDefaultAsync(r => r.Id == orderId);
            if (existing == null)
            {
                existing = new AggregatedStockOrderEntity { Id = orderId, CreatedAt = DateTimeOffset.UtcNow };
                await db.AggregatedStockOrders.AddAsync(existing);
            }

            var cp = GetProp(root, "Code");
            if (cp.ValueKind != JsonValueKind.Undefined) existing.Code = cp.GetString();

            var wp = GetProp(root, "WarehouseId");
            if (wp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(wp.GetString(), out var wG)) existing.WarehouseId = wG;

            var pp = GetProp(root, "PartnerId");
            if (pp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(pp.GetString(), out var pG)) existing.PartnerId = pG;

            var usrp = GetProp(root, "UserId");
            if (usrp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(usrp.GetString(), out var usrG)) existing.UserId = usrG;

            var unp = GetProp(root, "UserName");
            if (unp.ValueKind != JsonValueKind.Undefined) existing.UserName = unp.GetString();

            var comp = GetProp(root, "IsCompleted");
            if (comp.ValueKind == JsonValueKind.True || comp.ValueKind == JsonValueKind.False) existing.IsCompleted = comp.GetBoolean();

            var dis = GetProp(root, "IsDisabled");
            if (dis.ValueKind == JsonValueKind.True || dis.ValueKind == JsonValueKind.False) existing.IsDisabled = dis.GetBoolean();

            var gp = GetProp(root, "Group");
            if (gp.ValueKind != JsonValueKind.Undefined) existing.GroupName = gp.GetString();

            var dp = GetProp(root, "Description");
            if (dp.ValueKind != JsonValueKind.Undefined) existing.Description = dp.GetString();

            existing.Date = DateTimeOffset.UtcNow;
            existing.UpdatedAt = DateTimeOffset.UtcNow;

            var linesElem = GetProp(root, "Lines");
            if (linesElem.ValueKind == JsonValueKind.Array)
            {
                db.AggregatedStockOrderLines.RemoveRange(existing.Lines);
                foreach (var le in linesElem.EnumerateArray())
                {
                    var lidProp = GetProp(le, "Id");
                    var lStockProp = GetProp(le, "StockId");
                    var lUnitProp = GetProp(le, "UnitId");
                    var lOrdersProp = GetProp(le, "Orders");

                    var line = new AggregatedStockOrderLineEntity
                    {
                        Id = lidProp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(lidProp.GetString(), out var lG) ? lG : Guid.NewGuid(),
                        AggregatedStockOrderId = orderId,
                        StockId = lStockProp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(lStockProp.GetString(), out var stG) ? stG : null,
                        UnitId = lUnitProp.ValueKind != JsonValueKind.Undefined && Guid.TryParse(lUnitProp.GetString(), out var uG) ? uG : null,
                        OrdersJson = lOrdersProp.ValueKind == JsonValueKind.Object ? lOrdersProp.GetRawText() : "{}"
                    };
                    await db.AggregatedStockOrderLines.AddAsync(line);
                }
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { id = orderId });
        });

        group.MapDelete("/{id}", async (string id, MermerDbContext db) =>
        {
            if (Guid.TryParse(id, out var guid))
            {
                var o = await db.AggregatedStockOrders.FirstOrDefaultAsync(x => x.Id == guid);
                if (o != null)
                {
                    db.AggregatedStockOrders.Remove(o);
                    await db.SaveChangesAsync();
                }
            }
            return Results.Ok();
        });

        return app;
    }
}