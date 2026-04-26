using System;

namespace Payhas.Binyat.Data.Postgres.Entities;

/// <summary>
/// Invoice Overhead entity — additional costs (shipping, customs, etc.).
/// Maps to: Payhas.Binyat.Transactions.Models.StockTransactionOverhead
/// </summary>
public class InvoiceOverheadEntity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }

    /// <summary>NUMERIC(18,4) — overhead amount.</summary>
    public decimal Amount { get; set; }

    public Guid? CurrencyId { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public InvoiceEntity Invoice { get; set; } = null!;
    public CurrencyEntity? Currency { get; set; }
}
