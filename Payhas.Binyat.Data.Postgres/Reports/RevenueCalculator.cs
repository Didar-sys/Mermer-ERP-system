using System;
using System.Collections.Generic;
using System.Linq;
using Payhas.Binyat.Data.Postgres.Models;

namespace Payhas.Binyat.Data.Postgres.Reports;

/// <summary>
/// Running weighted-average cost calculator. Shared between the PostgreSQL
/// and SQLite repositories so both backends produce identical numbers.
///
/// The legacy bug we're killing here:
///   Sell → return → resell  ⇒  the second sale showed cost = 0 → 100% profit.
///
/// Why it happened:
///   The old NoSQL pipeline didn't preserve "the cost of the unit that was
///   coming back" when a SalesReturn was processed — it treated the return
///   as a free incoming and the next sale of that stock saw zero stock
///   value left in the bucket.
///
/// Fix:
///   * Purchase       → unit_cost = invoice_lines.price (cost of the buy)
///   * SalesReturn    → unit_cost = original Sales line's cost
///                      (looked up via invoice_lines.source_id)
///   * Sales / PurchaseReturn → unit_cost = current running average
///
/// Algorithm:
///   per (warehouse, stock) running tuple (qty, value, avg_cost):
///     income  → qty += dq;  value += dq * unit_cost_in;  avg = value / qty
///     expense → qty -= dq;  value -= dq * avg;           avg unchanged
///   We also remember the cost we used for every line_id, so when a future
///   SalesReturn with source_id arrives, we can return the *exact* same
///   unit cost that left the warehouse on that original sale.
/// </summary>
public static class RevenueCalculator
{
    public sealed class StockMovement
    {
        public DateTime  Date          { get; set; }
        public string    InvoiceId     { get; set; } = "";
        public string?   InvoiceCode   { get; set; }
        public InvoiceType InvoiceType { get; set; }

        public string    LineId        { get; set; } = "";
        public string?   SourceLineId  { get; set; }
        public string?   StockId       { get; set; }
        public string?   StockCode     { get; set; }
        public string?   StockName     { get; set; }

        public string?   WarehouseId   { get; set; }
        public string?   WarehouseName { get; set; }

        public decimal   Quantity      { get; set; }
        public decimal   UnitPrice     { get; set; }
    }

    /// <summary>
    /// Builds a Revenue Report for a chronological stream of stock movements.
    /// </summary>
    /// <param name="movements">
    /// Every <c>invoice_lines</c> row in the system that affected stock,
    /// sorted by <c>(date asc, line_id asc)</c>. Tail-truncating the stream
    /// before <paramref name="from"/> would corrupt the running average,
    /// so callers MUST pass the full history up to <paramref name="till"/>.
    /// </param>
    /// <param name="from">Inclusive lower bound of the report window.</param>
    /// <param name="till">Inclusive upper bound of the report window.</param>
    public static List<RevenueReportRow> Build(
        IEnumerable<StockMovement> movements,
        DateTime from,
        DateTime till)
    {
        var report = new List<RevenueReportRow>();
        var state    = new Dictionary<(string warehouse, string stock), (decimal qty, decimal value)>();
        var lineCost = new Dictionary<string, decimal>();

        foreach (var m in movements)
        {
            if (m.StockId is null || m.WarehouseId is null) continue;
            var key = (m.WarehouseId, m.StockId);
            var (qty, value) = state.TryGetValue(key, out var s) ? s : (0m, 0m);
            var avgBefore   = qty > 0 ? value / qty : 0m;

            decimal unitCost;
            bool    isIncome;

            switch (m.InvoiceType)
            {
                case InvoiceType.Purchase:
                    isIncome = true;
                    unitCost = m.UnitPrice;
                    break;

                case InvoiceType.SalesReturn:
                    isIncome = true;
                    // Authoritative path: the line carries source_id to the
                    // original Sales line whose cost we already recorded.
                    if (m.SourceLineId != null && lineCost.TryGetValue(m.SourceLineId, out var origCost))
                        unitCost = origCost;
                    else
                        unitCost = avgBefore;     // best-effort fallback
                    break;

                case InvoiceType.Sales:
                case InvoiceType.PurchaseReturn:
                    isIncome = false;
                    unitCost = avgBefore;
                    break;

                default:
                    continue;
            }

            if (isIncome)
            {
                qty   += m.Quantity;
                value += m.Quantity * unitCost;
            }
            else
            {
                qty   -= m.Quantity;
                value -= m.Quantity * unitCost;
                if (qty < 0) qty = 0;             // defensive: never negative
                if (value < 0) value = 0;
            }
            state[key] = (qty, value);

            lineCost[m.LineId] = unitCost;

            // Only Sales / PurchaseReturn rows produce report entries
            // (they're the ones that "earn" — or lose — money).
            var isReportable = m.InvoiceType is InvoiceType.Sales or InvoiceType.PurchaseReturn;
            if (!isReportable) continue;
            if (m.Date < from || m.Date > till) continue;

            var revenue = m.Quantity * m.UnitPrice;
            var cost    = m.Quantity * unitCost;
            // For PurchaseReturn the "revenue" is what we got back from the
            // supplier, the "cost" is what we paid originally — the sign of
            // profit on a return is intentionally the same formula.
            var profit  = revenue - cost;

            report.Add(new RevenueReportRow
            {
                Date          = m.Date,
                InvoiceId     = m.InvoiceId,
                InvoiceCode   = m.InvoiceCode,
                InvoiceType   = m.InvoiceType,
                LineId        = m.LineId,
                StockId       = m.StockId,
                StockCode     = m.StockCode,
                StockName     = m.StockName,
                WarehouseId   = m.WarehouseId,
                WarehouseName = m.WarehouseName,
                Quantity      = m.Quantity,
                UnitPrice     = m.UnitPrice,
                Revenue       = decimal.Round(revenue, 4),
                UnitCost      = decimal.Round(unitCost, 4),
                CostTotal     = decimal.Round(cost,    4),
                Profit        = decimal.Round(profit,  4),
                ProfitPercent = revenue == 0 ? 0 : decimal.Round(profit / revenue * 100, 2)
            });
        }

        return report;
    }
}
