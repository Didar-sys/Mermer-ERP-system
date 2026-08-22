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

public static class AggregatedReportsEndpoints
{
    public static IEndpointRouteBuilder MapAggregatedReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aggregated-reports").WithTags("AggregatedReports");

        group.MapGet("/", async (HttpRequest req, MermerDbContext db, CancellationToken ct) =>
        {
            DateTimeOffset dateFrom = DateTimeOffset.MinValue;
            DateTimeOffset dateTill = DateTimeOffset.MaxValue;

            string? fromStr = req.Query["dateFrom"].FirstOrDefault();
            if (!string.IsNullOrEmpty(fromStr) && DateTimeOffset.TryParse(fromStr.Replace(" ", "+"), out var pf))
                dateFrom = pf.ToUniversalTime();

            string? tillStr = req.Query["dateTill"].FirstOrDefault();
            if (!string.IsNullOrEmpty(tillStr) && DateTimeOffset.TryParse(tillStr.Replace(" ", "+"), out var pt))
                dateTill = pt.ToUniversalTime();

            var officeIds = req.Query["officeId"]
                .Select(x => Guid.TryParse(x, out var g) ? (Guid?)g : null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            if (!officeIds.Any())
            {
                return Results.Ok(new
                {
                    StocksReport = new { StartingBalance = 0m, Income = 0m, Expense = 0m, Lines = new List<object>() },
                    FundsReport = new { StartingBalance = 0m, Income = 0m, Expense = 0m, Lines = new List<object>() },
                    PartnersReport = new { StartingBalance = 0m, Debit = 0m, Credit = 0m, Lines = new List<object>() }
                });
            }

            // --- 1. Сбор идентификаторов связанных складов и касс ---
            var whIds = await db.Warehouses.Where(w => w.OfficeId.HasValue && officeIds.Contains(w.OfficeId.Value)).Select(w => w.Id).ToListAsync(ct);
            var depIds = await db.Depositories.Where(d => d.OfficeId.HasValue && officeIds.Contains(d.OfficeId.Value)).Select(d => d.Id).ToListAsync(ct);

            // --- 2. Склады (Stocks) ---
            var allStocks = new List<StockItem>();

            var stockInvs = await db.InvoiceLines
                .Where(l => l.Invoice.WarehouseId.HasValue && whIds.Contains(l.Invoice.WarehouseId.Value) && l.Invoice.IsCompleted && !l.Invoice.IsDisabled)
                .Select(l => new StockItem(l.Invoice.Date, l.Invoice.InvoiceType, l.Quantity, l.Invoice.InvoiceType == "Purchase" || l.Invoice.InvoiceType == "SalesReturn"))
                .ToListAsync(ct);
            allStocks.AddRange(stockInvs);

            var stockSlips = await db.StockSlipLines
                .Where(l => l.StockSlip.WarehouseId.HasValue && whIds.Contains(l.StockSlip.WarehouseId.Value) && l.StockSlip.IsCompleted)
                .Select(l => new StockItem(l.StockSlip.Date, l.StockSlip.SlipType, l.Quantity, l.StockSlip.SlipType == "StockOpening" || l.StockSlip.SlipType == "RevisionExceed"))
                .ToListAsync(ct);
            allStocks.AddRange(stockSlips);

            var stockTrOut = await db.StockTransferLines
                .Where(l => l.StockTransfer.WarehouseId.HasValue && whIds.Contains(l.StockTransfer.WarehouseId.Value) && l.StockTransfer.IsCompleted && !l.StockTransfer.IsDisabled)
                .Select(l => new StockItem(l.StockTransfer.Date, "StockTransferSource", l.Quantity, false))
                .ToListAsync(ct);
            allStocks.AddRange(stockTrOut);

            var stockTrIn = await db.StockTransferLines
                .Where(l => l.StockTransfer.DestinationWarehouseId.HasValue && whIds.Contains(l.StockTransfer.DestinationWarehouseId.Value) && l.StockTransfer.IsCompleted && !l.StockTransfer.IsDisabled)
                .Select(l => new StockItem(l.StockTransfer.Date, "StockTransferDestination", l.ReceivedQuantity, true))
                .ToListAsync(ct);
            allStocks.AddRange(stockTrIn);

            var stocksStart = allStocks.Where(x => x.Dt < dateFrom).Sum(x => x.IsInc ? x.Qty : -x.Qty);
            var stocksPeriod = allStocks.Where(x => x.Dt >= dateFrom && x.Dt <= dateTill).ToList();
            var stocksLines = stocksPeriod.GroupBy(x => x.Type).Select(g => new
            {
                Type = g.Key,
                Income = g.Sum(x => x.IsInc ? x.Qty : 0m),
                Expense = g.Sum(x => !x.IsInc ? x.Qty : 0m),
                Effect = g.Sum(x => x.IsInc ? x.Qty : -x.Qty)
            }).ToList();

            // --- 3. Касса и Фонды (Funds) ---
            var allFunds = new List<FundItem>();

            var fundsSlips = await db.FundsSlipLines
                .Where(l => l.FundsSlip.DepositoryId.HasValue && depIds.Contains(l.FundsSlip.DepositoryId.Value) && l.FundsSlip.IsCompleted && !l.FundsSlip.IsDisabled)
                .Select(l => new FundItem(l.FundsSlip.Date, l.FundsSlip.FundsSlipType, l.Amount, l.FundsSlip.FundsSlipType == "Income"))
                .ToListAsync(ct);
            allFunds.AddRange(fundsSlips);

            var fundsTrOut = await db.FundsTransferLines
                .Where(l => l.FundsTransfer.FromDepositoryId.HasValue && depIds.Contains(l.FundsTransfer.FromDepositoryId.Value) && l.FundsTransfer.IsCompleted && !l.FundsTransfer.IsDisabled)
                .Select(l => new FundItem(l.FundsTransfer.Date, "FundsTransferOut", l.Amount, false))
                .ToListAsync(ct);
            allFunds.AddRange(fundsTrOut);

            var fundsTrIn = await db.FundsTransferLines
                .Where(l => l.FundsTransfer.ToDepositoryId.HasValue && depIds.Contains(l.FundsTransfer.ToDepositoryId.Value) && l.FundsTransfer.IsCompleted && !l.FundsTransfer.IsDisabled)
                .Select(l => new FundItem(l.FundsTransfer.Date, "FundsTransferIn", l.ReceivedAmount, true))
                .ToListAsync(ct);
            allFunds.AddRange(fundsTrIn);

            var invoicePayments = await db.InvoicePayments
                .Where(p => p.Invoice.DepositoryId.HasValue && depIds.Contains(p.Invoice.DepositoryId.Value) && p.Invoice.IsCompleted && !p.Invoice.IsDisabled)
                .Select(p => new FundItem(p.Invoice.Date, p.Invoice.InvoiceType + "Payment", p.Amount, p.Invoice.InvoiceType == "Sales" || p.Invoice.InvoiceType == "PurchaseReturn"))
                .ToListAsync(ct);
            allFunds.AddRange(invoicePayments);

            var fundsStart = allFunds.Where(x => x.Dt < dateFrom).Sum(x => x.IsInc ? x.Amt : -x.Amt);
            var fundsPeriod = allFunds.Where(x => x.Dt >= dateFrom && x.Dt <= dateTill).ToList();
            var fundsLines = fundsPeriod.GroupBy(x => x.Type).Select(g => new
            {
                Type = g.Key,
                Income = g.Sum(x => x.IsInc ? x.Amt : 0m),
                Expense = g.Sum(x => !x.IsInc ? x.Amt : 0m),
                Effect = g.Sum(x => x.IsInc ? x.Amt : -x.Amt)
            }).ToList();

            // --- 4. Контрагенты (Partners) ---
            var allPartners = new List<PartnerItem>();

            var partSlips = await db.PartnerSlipLines
                .Where(l => l.PartnerSlip.OfficeId.HasValue && officeIds.Contains(l.PartnerSlip.OfficeId.Value) && !l.PartnerSlip.IsDisabled)
                .Select(l => new PartnerItem(l.PartnerSlip.Date, l.PartnerSlip.SlipType, l.DebitAmount, l.CreditAmount))
                .ToListAsync(ct);
            allPartners.AddRange(partSlips);

            var partTrs = await db.PartnerTransferLines
                .Where(l => l.OfficeId.HasValue && officeIds.Contains(l.OfficeId.Value) && !l.PartnerTransfer.IsDisabled)
                .Select(l => new PartnerItem(l.PartnerTransfer.Date, "PartnerTransfer", l.DebitAmount, l.CreditAmount))
                .ToListAsync(ct);
            allPartners.AddRange(partTrs);

            var rawInvs = await db.InvoiceLines
                .Where(l => l.Invoice.OfficeId.HasValue && officeIds.Contains(l.Invoice.OfficeId.Value) && l.Invoice.IsCompleted && !l.Invoice.IsDisabled && l.Invoice.PartnerId.HasValue)
                .Select(l => new { l.Invoice.Date, l.Invoice.InvoiceType, Total = l.Quantity * l.Price, IsDebit = l.Invoice.InvoiceType == "Sales" || l.Invoice.InvoiceType == "PurchaseReturn" })
                .ToListAsync(ct);

            allPartners.AddRange(rawInvs.Select(l => new PartnerItem(l.Date, l.InvoiceType, l.IsDebit ? l.Total : 0m, !l.IsDebit ? l.Total : 0m)));

            var rawInvPaymentsForPartner = await db.InvoicePayments
                .Where(p => p.Invoice.OfficeId.HasValue && officeIds.Contains(p.Invoice.OfficeId.Value) && p.Invoice.IsCompleted && !p.Invoice.IsDisabled && p.Invoice.PartnerId.HasValue)
                .Select(p => new { p.Invoice.Date, Type = p.Invoice.InvoiceType + "Payment", Total = p.Amount, IsDebit = p.Invoice.InvoiceType == "Purchase" || p.Invoice.InvoiceType == "SalesReturn" })
                .ToListAsync(ct);

            allPartners.AddRange(rawInvPaymentsForPartner.Select(p => new PartnerItem(p.Date, p.Type, p.IsDebit ? p.Total : 0m, !p.IsDebit ? p.Total : 0m)));

            var partnersStart = allPartners.Where(x => x.Dt < dateFrom).Sum(x => x.Deb - x.Cre);
            var partnersPeriod = allPartners.Where(x => x.Dt >= dateFrom && x.Dt <= dateTill).ToList();
            var partnersLines = partnersPeriod.GroupBy(x => x.Type).Select(g => new
            {
                Type = g.Key,
                Debit = g.Sum(x => x.Deb),
                Credit = g.Sum(x => x.Cre),
                Effect = g.Sum(x => x.Deb - x.Cre)
            }).ToList();

            // --- 5. Формирование ответа ---
            return Results.Ok(new
            {
                StocksReport = new
                {
                    StartingBalance = stocksStart,
                    Income = stocksLines.Sum(x => x.Income),
                    Expense = stocksLines.Sum(x => x.Expense),
                    Lines = stocksLines
                },
                FundsReport = new
                {
                    StartingBalance = fundsStart,
                    Income = fundsLines.Sum(x => x.Income),
                    Expense = fundsLines.Sum(x => x.Expense),
                    Lines = fundsLines
                },
                PartnersReport = new
                {
                    StartingBalance = partnersStart,
                    Debit = partnersLines.Sum(x => x.Debit),
                    Credit = partnersLines.Sum(x => x.Credit),
                    Lines = partnersLines
                }
            });
        });

        return app;
    }

    private sealed record StockItem(DateTimeOffset Dt, string Type, decimal Qty, bool IsInc);
    private sealed record FundItem(DateTimeOffset Dt, string Type, decimal Amt, bool IsInc);
    private sealed record PartnerItem(DateTimeOffset Dt, string Type, decimal Deb, decimal Cre);
}