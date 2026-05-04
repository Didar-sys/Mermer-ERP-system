using System;

namespace Payhas.Binyat.Data.Postgres.Models;

/// <summary>
/// One row of the Revenue / Profit-and-loss report.
/// Shown to the user as: "what we sold, for how much, at what cost,
/// what's the profit".
///
/// CRITICAL — the bug this DTO addresses:
///   In the legacy system, when a sold item was returned and re-sold,
///   the second sale showed Cost = 0 → "100% profit".
///   <see cref="UnitCost"/> here is computed via running weighted-average,
///   honoring SalesReturn → original-cost lookup through
///   <c>invoice_lines.source_id</c>. So the second sale carries the same
///   cost as the first, and Profit reflects reality.
/// </summary>
public sealed class RevenueReportRow
{
    public DateTime  Date          { get; set; }
    public string?   InvoiceId     { get; set; }
    public string?   InvoiceCode   { get; set; }
    public InvoiceType InvoiceType { get; set; }

    public string?   LineId        { get; set; }
    public string?   StockId       { get; set; }
    public string?   StockCode     { get; set; }
    public string?   StockName     { get; set; }

    public string?   WarehouseId   { get; set; }
    public string?   WarehouseName { get; set; }

    public decimal   Quantity      { get; set; }
    public decimal   UnitPrice     { get; set; }
    public decimal   Revenue       { get; set; }   // Quantity * UnitPrice

    public decimal   UnitCost      { get; set; }
    public decimal   CostTotal     { get; set; }   // Quantity * UnitCost

    public decimal   Profit        { get; set; }   // Revenue - CostTotal

    /// <summary>Profit / Revenue × 100 (or 0 when Revenue is 0).</summary>
    public decimal   ProfitPercent { get; set; }
}
