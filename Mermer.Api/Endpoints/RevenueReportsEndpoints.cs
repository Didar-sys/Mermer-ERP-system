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

public static class RevenueReportsEndpoints
{
    public static IEndpointRouteBuilder MapRevenueReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/revenue-reports").WithTags("RevenueReports");

        group.MapGet("/", async (HttpRequest req, MermerDbContext db, CancellationToken ct) =>
        {
            DateTimeOffset dateFrom = DateTimeOffset.MinValue;
            DateTimeOffset dateTill = DateTimeOffset.MaxValue;

            if (DateTimeOffset.TryParse(req.Query["dateFrom"].FirstOrDefault()?.Replace(" ", "+"), out var pf))
                dateFrom = pf.ToUniversalTime();
            if (DateTimeOffset.TryParse(req.Query["dateTill"].FirstOrDefault()?.Replace(" ", "+"), out var pt))
                dateTill = pt.ToUniversalTime();

            // Защита от перевернутых дат
            if (dateFrom > dateTill)
            {
                var temp = dateFrom;
                dateFrom = dateTill;
                dateTill = temp;
            }

            var whIds = req.Query["warehouseId"]
                .Select(x => Guid.TryParse(x, out var g) ? (Guid?)g : null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            if (!whIds.Any())
            {
                return Results.Ok(new List<RevenueReportDto>());
            }

            // 1. Выгружаем Продажи (Sales) и Возвраты от покупателей (SalesReturn)
            var targetLines = await db.InvoiceLines
                .Include(l => l.Invoice)
                .Include(l => l.Stock)
                .Where(l => l.Invoice.IsCompleted && !l.Invoice.IsDisabled)
                .Where(l => l.Invoice.WarehouseId.HasValue && whIds.Contains(l.Invoice.WarehouseId.Value))
                .Where(l => l.Invoice.Date >= dateFrom && l.Invoice.Date <= dateTill)
                .Where(l => l.Invoice.InvoiceType == "Sales" || l.Invoice.InvoiceType == "SalesReturn")
                .ToListAsync(ct);

            if (!targetLines.Any())
                return Results.Ok(new List<RevenueReportDto>());

            var stockIds = targetLines.Where(l => l.StockId.HasValue).Select(l => l.StockId!.Value).Distinct().ToList();

            // 2. Выгружаем прайс-лист для "Рекомендованной цены"
            var stockPrices = await db.StockPrices
                .Where(p => stockIds.Contains(p.StockId))
                .OrderByDescending(p => p.ValidFrom)
                .AsNoTracking()
                .ToListAsync(ct);

            // 3. Выгружаем Закупки для вычисления себестоимости (Cost of Goods Sold)
            var purchases = await db.InvoiceLines
                .Include(l => l.Invoice)
                .Where(l => l.StockId.HasValue && stockIds.Contains(l.StockId.Value) && l.Invoice.IsCompleted && !l.Invoice.IsDisabled)
                .Where(l => l.Invoice.InvoiceType == "Purchase" || l.Invoice.InvoiceType == "StockOpening" || l.Invoice.InvoiceType == "PurchaseReturn")
                .AsNoTracking()
                .ToListAsync(ct);

            // Считаем средневзвешенную закупочную цену для каждого товара
            var avgCosts = new Dictionary<Guid, decimal>();
            foreach (var sId in stockIds)
            {
                var pLines = purchases.Where(p => p.StockId == sId).ToList();
                decimal totalVal = pLines.Sum(p => (p.Invoice.InvoiceType == "PurchaseReturn" ? -1 : 1) * p.Quantity * p.Price);
                decimal totalQty = pLines.Sum(p => (p.Invoice.InvoiceType == "PurchaseReturn" ? -1 : 1) * p.Quantity);

                avgCosts[sId] = totalQty > 0 ? totalVal / totalQty : 0m;
            }

            // 4. Формируем DTO для интерфейса
            var results = new List<RevenueReportDto>();

            foreach (var line in targetLines)
            {
                if (!line.StockId.HasValue) continue;

                // Для продаж Quantity с плюсом, для возвратов - с минусом (чтобы отнять прибыль)
                decimal quantity = line.Invoice.InvoiceType == "SalesReturn" ? -line.Quantity : line.Quantity;

                // РАСЧЕТ СЕБЕСТОИМОСТИ (ИСПРАВЛЕНИЕ БАГА)
                decimal unitCost = 0m;

                // Если есть прямая ссылка на закупку или предыдущую продажу (SourceId)
                if (line.SourceId.HasValue)
                {
                    var sourceLine = purchases.FirstOrDefault(p => p.Id == line.SourceId.Value)
                                  ?? targetLines.FirstOrDefault(t => t.Id == line.SourceId.Value);

                    if (sourceLine != null && sourceLine.Price > 0)
                        unitCost = sourceLine.Price;
                    else
                        unitCost = avgCosts.GetValueOrDefault(line.StockId.Value, 0m);
                }
                else
                {
                    // Если связи нет, используем средневзвешенную цену закупки
                    unitCost = avgCosts.GetValueOrDefault(line.StockId.Value, 0m);
                }

                decimal initialCosts = quantity * unitCost;

                // Рекомендованная цена (из прайса на момент продажи)
                var recPrice = stockPrices.FirstOrDefault(p => p.StockId == line.StockId.Value && p.ValidFrom <= line.Invoice.Date)?.Price ?? 0m;

                results.Add(new RevenueReportDto
                {
                    Date = line.Invoice.Date.UtcDateTime,
                    WarehouseId = line.Invoice.WarehouseId.ToString(),
                    StockId = line.StockId.ToString(),
                    StockCode = line.Stock?.Code ?? "",
                    StockName = line.Stock?.Name ?? "",
                    StockType = line.Stock?.Type ?? "",
                    StockGroup = line.Stock?.Group ?? "",
                    StockTags = new List<string>(), // Можно заполнить, если нужно отображать в гриде
                    Quantity = quantity,
                    InitialCosts = initialCosts,
                    OverheadsCosts = 0m,
                    RecommendedPrice = recPrice,
                    ActualPrice = line.Price
                });
            }

            return Results.Ok(results.OrderBy(r => r.Date).ToList());
        });

        return app;
    }

    public class RevenueReportDto
    {
        public DateTime Date { get; set; }
        public string WarehouseId { get; set; } = string.Empty;
        public string StockId { get; set; } = string.Empty;
        public string StockCode { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
        public string StockUnit { get; set; } = string.Empty;
        public string StockType { get; set; } = string.Empty;
        public string StockGroup { get; set; } = string.Empty;
        public List<string> StockTags { get; set; } = new();

        public decimal Quantity { get; set; }
        public decimal InitialCosts { get; set; }
        public decimal OverheadsCosts { get; set; }
        public decimal RecommendedPrice { get; set; }
        public decimal ActualPrice { get; set; }
    }
}