using System;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Invoice Payment entity.
/// Maps to: Mermer.Commerce.Models.InvoicePayment
/// </summary>
public class InvoicePaymentEntity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }

    /// <summary>Payment or Change</summary>
    public string PaymentType { get; set; } = "Payment";

    /// <summary>NUMERIC(18,4) — payment amount.</summary>
    public decimal Amount { get; set; }

    public Guid? CurrencyId { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public InvoiceEntity Invoice { get; set; } = null!;
    public CurrencyEntity? Currency { get; set; }
}
