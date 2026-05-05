using System;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Invoice Line entity — product line within an invoice.
/// Maps to: InvoiceLine → StockTransactionLine → TransactionLine
/// </summary>
public class InvoiceLineEntity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid? SourceId { get; set; }
    public Guid? StockId { get; set; }
    public Guid? UnitId { get; set; }

    /// <summary>NUMERIC(18,4) — quantity of product.</summary>
    public decimal Quantity { get; set; }

    /// <summary>NUMERIC(18,4) — unit price.</summary>
    public decimal Price { get; set; }

    public Guid? CurrencyId { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public InvoiceEntity Invoice { get; set; } = null!;
    public StockEntity? Stock { get; set; }
    public StockUnitEntity? Unit { get; set; }
    public CurrencyEntity? Currency { get; set; }
}
