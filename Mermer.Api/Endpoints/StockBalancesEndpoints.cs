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

public static class StockBalancesEndpoints
{
    public static IEndpointRouteBuilder MapStockBalancesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stock-balances").WithTags("StockBalances");

        group.MapGet("/by-type", async (HttpRequest req, MermerDbContext db, CancellationToken ct) =>
        {
            DateTimeOffset dateFrom = DateTimeOffset.MinValue;
            DateTimeOffset dateTill = DateTimeOffset.MaxValue;

            string? fromStr = req.Query["dateFrom"].FirstOrDefault();
            if (!string.IsNullOrEmpty(fromStr) && DateTimeOffset.TryParse(fromStr.Replace(" ", "+"), out var pFrom))
                dateFrom = pFrom.ToUniversalTime();

            string? tillStr = req.Query["dateTill"].FirstOrDefault();
            if (!string.IsNullOrEmpty(tillStr) && DateTimeOffset.TryParse(tillStr.Replace(" ", "+"), out var pTill))
                dateTill = pTill.ToUniversalTime();

            bool aggregate = bool.TryParse(req.Query["aggregate"].FirstOrDefault(), out var agg) && agg;
            string? stockIdStr = req.Query["stockId"].FirstOrDefault();
            Guid? filterStockGuid = Guid.TryParse(stockIdStr, out var sG) ? sG : null;

            var whIds = req.Query["warehouseId"]
                .Select(x => Guid.TryParse(x, out var g) ? (Guid?)g : null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            // Загружаем активные товары со связанными данными
            var stocksQuery = db.Stocks
                .Include(s => s.Units)
                .Include(s => s.Prices)
                .AsSplitQuery()
                .AsNoTracking()
                .Where(s => !s.IsDisabled);

            if (filterStockGuid.HasValue)
                stocksQuery = stocksQuery.Where(s => s.Id == filterStockGuid.Value);

            var stocks = await stocksQuery.ToListAsync(ct);
            if (!stocks.Any()) return Results.Ok(new object[0]);

            // Загружаем документы движений
            var slipsQuery = db.StockSlips.Include(s => s.Lines).AsNoTracking().Where(s => !s.IsCompleted || s.IsCompleted);
            if (whIds.Any()) slipsQuery = slipsQuery.Where(s => s.WarehouseId.HasValue && whIds.Contains(s.WarehouseId.Value));
            var slips = await slipsQuery.ToListAsync(ct);

            var transfersQuery = db.StockTransfers.Include(t => t.Lines).AsNoTracking().Where(t => !t.IsDisabled);
            if (whIds.Any()) transfersQuery = transfersQuery.Where(t => (t.WarehouseId.HasValue && whIds.Contains(t.WarehouseId.Value)) || (t.DestinationWarehouseId.HasValue && whIds.Contains(t.DestinationWarehouseId.Value)));
            var transfers = await transfersQuery.ToListAsync(ct);

            var invoicesQuery = db.Invoices.Include(i => i.Lines).AsNoTracking().Where(i => !i.IsDisabled && i.IsCompleted);
            if (whIds.Any()) invoicesQuery = invoicesQuery.Where(i => i.WarehouseId.HasValue && whIds.Contains(i.WarehouseId.Value));
            var invoices = await invoicesQuery.ToListAsync(ct);

            // Собираем все плоские движения
            var allMovements = new List<StockMovementRecord>();

            foreach (var s in slips)
            {
                foreach (var l in s.Lines ?? Enumerable.Empty<Data.Postgres.Entities.StockSlipLineEntity>())
                {
                    if (!l.StockId.HasValue || !s.WarehouseId.HasValue) continue;
                    allMovements.Add(new StockMovementRecord(
                        s.WarehouseId.Value,
                        l.StockId.Value,
                        s.Date,
                        s.SlipType,
                        s.SlipType == "StockOpening" || s.SlipType == "RevisionExceed" ? l.Quantity : 0m,
                        s.SlipType != "StockOpening" && s.SlipType != "RevisionExceed" ? l.Quantity : 0m
                    ));
                }
            }

            foreach (var t in transfers)
            {
                foreach (var l in t.Lines ?? Enumerable.Empty<Data.Postgres.Entities.StockTransferLineEntity>())
                {
                    if (!l.StockId.HasValue) continue;
                    if (t.WarehouseId.HasValue)
                        allMovements.Add(new StockMovementRecord(t.WarehouseId.Value, l.StockId.Value, t.Date, "StockTransferSource", 0m, l.Quantity));
                    if (t.DestinationWarehouseId.HasValue)
                        allMovements.Add(new StockMovementRecord(t.DestinationWarehouseId.Value, l.StockId.Value, t.Date, "StockTransferDestination", l.ReceivedQuantity, 0m));
                }
            }

            foreach (var inv in invoices)
            {
                foreach (var l in inv.Lines ?? Enumerable.Empty<Data.Postgres.Entities.InvoiceLineEntity>())
                {
                    if (!l.StockId.HasValue || !inv.WarehouseId.HasValue) continue;
                    bool isIncome = inv.InvoiceType == "Purchase" || inv.InvoiceType == "SalesReturn";
                    allMovements.Add(new StockMovementRecord(
                        inv.WarehouseId.Value,
                        l.StockId.Value,
                        inv.Date,
                        inv.InvoiceType,
                        isIncome ? l.Quantity : 0m,
                        !isIncome ? l.Quantity : 0m
                    ));
                }
            }

            var result = new List<object>();

            if (aggregate)
            {
                foreach (var stock in stocks)
                {
                    var movements = allMovements.Where(m => m.StockId == stock.Id).ToList();
                    var startMovements = movements.Where(m => m.Date < dateFrom).ToList();
                    var periodMovements = movements.Where(m => m.Date >= dateFrom && m.Date <= dateTill).ToList();

                    decimal starting = startMovements.Sum(m => m.Income - m.Expense);
                    decimal inc = periodMovements.Sum(m => m.Income);
                    decimal exp = periodMovements.Sum(m => m.Expense);

                    if (starting == 0 && inc == 0 && exp == 0) continue;

                    var defaultUnit = stock.Units?.FirstOrDefault(u => u.IsDefault) ?? stock.Units?.FirstOrDefault();
                    var currentPrice = stock.Prices?.OrderByDescending(p => p.ValidFrom).FirstOrDefault();

                    result.Add(new
                    {
                        WarehouseId = (string?)null,
                        StockId = stock.Id.ToString(),
                        StockCode = stock.Code ?? "",
                        StockName = stock.Name ?? "",
                        StockShortName = stock.ShortName ?? "",
                        StockUnit = defaultUnit?.Name ?? "",
                        StockPrice = currentPrice?.Price ?? 0m,
                        StockCurrencyId = currentPrice?.CurrencyId?.ToString(),
                        StockType = stock.Type ?? "",
                        StockGroup = stock.Group ?? "",
                        StockTags = stock.Tags ?? Array.Empty<string>(),
                        StartingBalance = starting,
                        Income = inc,
                        Expense = exp,
                        StockOpening = periodMovements.Where(m => m.Type == "StockOpening").Sum(m => m.Income),
                        StockSpoilage = periodMovements.Where(m => m.Type == "StockSpoilage").Sum(m => m.Expense),
                        StockUsage = periodMovements.Where(m => m.Type == "StockUsage").Sum(m => m.Expense),
                        RevisionExceed = periodMovements.Where(m => m.Type == "RevisionExceed").Sum(m => m.Income),
                        RevisionDeficit = periodMovements.Where(m => m.Type == "RevisionDeficit").Sum(m => m.Expense),
                        StockTransferSource = periodMovements.Where(m => m.Type == "StockTransferSource").Sum(m => m.Expense),
                        StockTransferDestination = periodMovements.Where(m => m.Type == "StockTransferDestination").Sum(m => m.Income),
                        Sales = periodMovements.Where(m => m.Type == "Sales").Sum(m => m.Expense),
                        SalesReturn = periodMovements.Where(m => m.Type == "SalesReturn").Sum(m => m.Income),
                        Purchase = periodMovements.Where(m => m.Type == "Purchase").Sum(m => m.Income),
                        PurchaseReturn = periodMovements.Where(m => m.Type == "PurchaseReturn").Sum(m => m.Expense),
                        ResultingBalance = starting + inc - exp
                    });
                }
            }
            else
            {
                var targetWhIds = whIds.Any() ? whIds : allMovements.Select(m => m.WarehouseId).Distinct().ToList();

                foreach (var whId in targetWhIds)
                {
                    foreach (var stock in stocks)
                    {
                        var movements = allMovements.Where(m => m.WarehouseId == whId && m.StockId == stock.Id).ToList();
                        var startMovements = movements.Where(m => m.Date < dateFrom).ToList();
                        var periodMovements = movements.Where(m => m.Date >= dateFrom && m.Date <= dateTill).ToList();

                        decimal starting = startMovements.Sum(m => m.Income - m.Expense);
                        decimal inc = periodMovements.Sum(m => m.Income);
                        decimal exp = periodMovements.Sum(m => m.Expense);

                        if (starting == 0 && inc == 0 && exp == 0) continue;

                        var defaultUnit = stock.Units?.FirstOrDefault(u => u.IsDefault) ?? stock.Units?.FirstOrDefault();
                        var currentPrice = stock.Prices?.OrderByDescending(p => p.ValidFrom).FirstOrDefault();

                        result.Add(new
                        {
                            WarehouseId = whId.ToString(),
                            StockId = stock.Id.ToString(),
                            StockCode = stock.Code ?? "",
                            StockName = stock.Name ?? "",
                            StockShortName = stock.ShortName ?? "",
                            StockUnit = defaultUnit?.Name ?? "",
                            StockPrice = currentPrice?.Price ?? 0m,
                            StockCurrencyId = currentPrice?.CurrencyId?.ToString(),
                            StockType = stock.Type ?? "",
                            StockGroup = stock.Group ?? "",
                            StockTags = stock.Tags ?? Array.Empty<string>(),
                            StartingBalance = starting,
                            Income = inc,
                            Expense = exp,
                            StockOpening = periodMovements.Where(m => m.Type == "StockOpening").Sum(m => m.Income),
                            StockSpoilage = periodMovements.Where(m => m.Type == "StockSpoilage").Sum(m => m.Expense),
                            StockUsage = periodMovements.Where(m => m.Type == "StockUsage").Sum(m => m.Expense),
                            RevisionExceed = periodMovements.Where(m => m.Type == "RevisionExceed").Sum(m => m.Income),
                            RevisionDeficit = periodMovements.Where(m => m.Type == "RevisionDeficit").Sum(m => m.Expense),
                            StockTransferSource = periodMovements.Where(m => m.Type == "StockTransferSource").Sum(m => m.Expense),
                            StockTransferDestination = periodMovements.Where(m => m.Type == "StockTransferDestination").Sum(m => m.Income),
                            Sales = periodMovements.Where(m => m.Type == "Sales").Sum(m => m.Expense),
                            SalesReturn = periodMovements.Where(m => m.Type == "SalesReturn").Sum(m => m.Income),
                            Purchase = periodMovements.Where(m => m.Type == "Purchase").Sum(m => m.Income),
                            PurchaseReturn = periodMovements.Where(m => m.Type == "PurchaseReturn").Sum(m => m.Expense),
                            ResultingBalance = starting + inc - exp
                        });
                    }
                }
            }

            return Results.Ok(result);
        });

        group.MapGet("/by-date-warehouses", async (HttpRequest req, MermerDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(new object[0]);
        });

        group.MapGet("/aggregated", async (HttpRequest req, MermerDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(new
            {
                StartingBalance = 0,
                Income = 0,
                Expense = 0,
                Lines = new object[0]
            });
        });

        return app;
    }

    private record StockMovementRecord(Guid WarehouseId, Guid StockId, DateTimeOffset Date, string Type, decimal Income, decimal Expense);
}