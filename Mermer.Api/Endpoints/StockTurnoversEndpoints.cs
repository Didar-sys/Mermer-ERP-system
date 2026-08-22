using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;

namespace Mermer.Api.Endpoints;

public static class StockTurnoversEndpoints
{
    public static IEndpointRouteBuilder MapStockTurnoversEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stock-turnovers").WithTags("StockTurnovers");

        group.MapGet("/", async (HttpRequest req, MermerDbContext db, CancellationToken ct) =>
        {
            string? warehouseIdStr = req.Query["warehouseId"].FirstOrDefault();
            Guid? warehouseId = Guid.TryParse(warehouseIdStr, out var w) && w != Guid.Empty ? w : null;

            // 1. Приход
            var incQuery = db.InvoiceLines.Where(l => l.Invoice.IsCompleted && !l.Invoice.IsDisabled && (l.Invoice.InvoiceType == "Purchase" || l.Invoice.InvoiceType == "SalesReturn"))
                .Select(l => new { Wh = l.Invoice.WarehouseId, St = l.StockId, Qty = l.Quantity });

            var incSlip = db.StockSlipLines.Where(l => l.StockSlip.IsCompleted && (l.StockSlip.SlipType == "StockOpening" || l.StockSlip.SlipType == "RevisionExceed"))
                .Select(l => new { Wh = l.StockSlip.WarehouseId, St = l.StockId, Qty = l.Quantity });

            var incTrIn = db.StockTransferLines.Where(l => l.StockTransfer.IsCompleted && !l.StockTransfer.IsDisabled)
                .Select(l => new { Wh = l.StockTransfer.DestinationWarehouseId, St = l.StockId, Qty = l.ReceivedQuantity });

            var allInc = incQuery.Concat(incSlip).Concat(incTrIn).Where(x => x.Wh.HasValue && x.St.HasValue);
            if (warehouseId.HasValue) allInc = allInc.Where(x => x.Wh == warehouseId.Value);

            var incomeSums = await allInc.GroupBy(x => new { Wh = x.Wh!.Value, St = x.St!.Value })
                .Select(g => new { WarehouseId = g.Key.Wh, StockId = g.Key.St, Income = g.Sum(x => x.Qty) }).ToListAsync(ct);

            // 2. Расход
            var expQuery = db.InvoiceLines.Where(l => l.Invoice.IsCompleted && !l.Invoice.IsDisabled && (l.Invoice.InvoiceType == "Sales" || l.Invoice.InvoiceType == "PurchaseReturn"))
                .Select(l => new { Wh = l.Invoice.WarehouseId, St = l.StockId, Qty = l.Quantity });

            var expSlip = db.StockSlipLines.Where(l => l.StockSlip.IsCompleted && (l.StockSlip.SlipType != "StockOpening" && l.StockSlip.SlipType != "RevisionExceed"))
                .Select(l => new { Wh = l.StockSlip.WarehouseId, St = l.StockId, Qty = l.Quantity });

            var expTrOut = db.StockTransferLines.Where(l => l.StockTransfer.IsCompleted && !l.StockTransfer.IsDisabled)
                .Select(l => new { Wh = l.StockTransfer.WarehouseId, St = l.StockId, Qty = l.Quantity });

            var allExp = expQuery.Concat(expSlip).Concat(expTrOut).Where(x => x.Wh.HasValue && x.St.HasValue);
            if (warehouseId.HasValue) allExp = allExp.Where(x => x.Wh == warehouseId.Value);

            var expSums = await allExp.GroupBy(x => new { Wh = x.Wh!.Value, St = x.St!.Value })
                .Select(g => new { WarehouseId = g.Key.Wh, StockId = g.Key.St, Expense = g.Sum(x => x.Qty) }).ToListAsync(ct);

            // 3. Продажи
            var salesQuery = db.InvoiceLines.Where(l => l.Invoice.IsCompleted && !l.Invoice.IsDisabled && l.Invoice.InvoiceType == "Sales")
                .Select(l => new { Wh = l.Invoice.WarehouseId, St = l.StockId, Qty = l.Quantity });
            if (warehouseId.HasValue) salesQuery = salesQuery.Where(x => x.Wh == warehouseId.Value);

            var salesSums = await salesQuery.Where(x => x.Wh.HasValue && x.St.HasValue)
                .GroupBy(x => new { Wh = x.Wh!.Value, St = x.St!.Value })
                .Select(g => new { WarehouseId = g.Key.Wh, StockId = g.Key.St, Sold = g.Sum(x => x.Qty) }).ToListAsync(ct);

            // 4. Сбор ключей и получение номенклатуры
            var keys = incomeSums.Select(x => new { x.WarehouseId, x.StockId })
                .Union(expSums.Select(x => new { x.WarehouseId, x.StockId }))
                .Distinct().ToList();

            var nonNullStockIds = keys.Select(x => x.StockId).Distinct().ToList();

            var stocks = await db.Stocks
                .Where(s => nonNullStockIds.Contains(s.Id))
                .AsNoTracking()
                .ToDictionaryAsync(s => s.Id, s => s, ct);

            var result = keys.Select(k => {
                stocks.TryGetValue(k.StockId, out var stock);
                decimal inc = incomeSums.FirstOrDefault(x => x.WarehouseId == k.WarehouseId && x.StockId == k.StockId)?.Income ?? 0m;
                decimal exp = expSums.FirstOrDefault(x => x.WarehouseId == k.WarehouseId && x.StockId == k.StockId)?.Expense ?? 0m;
                decimal sold = salesSums.FirstOrDefault(x => x.WarehouseId == k.WarehouseId && x.StockId == k.StockId)?.Sold ?? 0m;

                return new StockTurnoverDataDto
                {
                    WarehouseId = k.WarehouseId.ToString(),
                    StockId = k.StockId.ToString(),
                    StockCode = stock?.Code ?? "N/A",
                    StockName = stock?.Name ?? "N/A",
                    StockType = stock?.Type ?? "",
                    StockGroup = stock?.Group ?? "",
                    StockTags = stock?.Tags ?? Array.Empty<string>(),
                    Income = inc,
                    Expense = exp,
                    Sold = sold
                };
            }).ToList();

            return Results.Ok(result);
        });

        return app;
    }

    public class StockTurnoverDataDto
    {
        public string WarehouseId { get; set; } = string.Empty;
        public string StockId { get; set; } = string.Empty;
        public string StockCode { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
        public string StockType { get; set; } = string.Empty;
        public string StockGroup { get; set; } = string.Empty;
        public string[] StockTags { get; set; } = Array.Empty<string>();
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Sold { get; set; }
    }
}