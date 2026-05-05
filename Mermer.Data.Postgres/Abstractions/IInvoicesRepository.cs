using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mermer.Data.Postgres.Models;

namespace Mermer.Data.Postgres.Abstractions;

/// <summary>
/// Invoice CRUD + financial reporting. All money math happens server-side
/// in NUMERIC(18,4); Flat / Percentage discounts and overheads are honored.
/// </summary>
public interface IInvoicesRepository
{
    Task<Invoice?> GetAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Aggregated list with subtotal / discounts / overheads / grand total /
    /// payments / left, computed via per-child-table CTEs to avoid Cartesian
    /// inflation on multi-line / multi-discount / multi-payment invoices.
    /// </summary>
    /// <param name="displayCurrencyId">
    /// Optional — when set, all amounts (lines, discounts, payments,
    /// overheads) are converted into this currency using the
    /// <c>invoice_currency_convertions</c> snapshot stored on each invoice.
    /// When null, raw amounts are returned (legacy behaviour).
    /// </param>
    Task<IReadOnlyList<InvoiceInfo>> GetInfoAsync(
        DateTime from, DateTime till,
        string? displayCurrencyId = null,
        CancellationToken ct = default);

    Task<int> CountInfoAsync(DateTime from, DateTime till, CancellationToken ct = default);

    /// <summary>Same as <see cref="GetInfoAsync"/> but enriched with partner debit/credit ledger totals.</summary>
    Task<IReadOnlyList<InvoicePaymentInfo>> GetPaymentInfoAsync(
        DateTime from, DateTime till, string? officeId, string? partnerId,
        string? displayCurrencyId = null,
        CancellationToken ct = default);

    Task<int> CountPaymentInfoAsync(
        DateTime from, DateTime till, string? officeId, string? partnerId, CancellationToken ct = default);

    Task<Invoice> CreateAsync(Invoice model, CancellationToken ct = default);
    Task<Invoice> UpdateAsync(Invoice model, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Revenue / profit-and-loss report.
    ///
    /// Per-line breakdown of every Sale / PurchaseReturn in the window with
    /// honest unit cost (running weighted-average, with SalesReturn lookups
    /// through <c>source_id</c>). This is what fixes the "100% profit on a
    /// resold-after-return item" bug from the legacy system.
    /// </summary>
    /// <param name="from">Inclusive lower bound of the report window.</param>
    /// <param name="till">Inclusive upper bound of the report window.</param>
    /// <param name="warehouseId">Optional warehouse filter.</param>
    Task<IReadOnlyList<RevenueReportRow>> GetRevenueReportAsync(
        DateTime from, DateTime till, string? warehouseId = null, CancellationToken ct = default);
}
