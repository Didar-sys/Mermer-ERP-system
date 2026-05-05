using System;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Invoice Discount entity.
/// Maps to: Mermer.Commerce.Models.InvoiceDiscount
/// </summary>
public class InvoiceDiscountEntity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }

    /// <summary>Flat or Percentage</summary>
    public string DiscountType { get; set; } = "Flat";

    /// <summary>NUMERIC(18,4) — discount amount or percentage value.</summary>
    public decimal Amount { get; set; }

    public string? Description { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public InvoiceEntity Invoice { get; set; } = null!;
}
