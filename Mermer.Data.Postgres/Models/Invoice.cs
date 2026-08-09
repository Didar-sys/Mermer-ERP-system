using System;
using System.Collections.Generic;

namespace Mermer.Data.Postgres.Models;

/// <summary>
/// Invoice (sale / purchase document) — POCO mirror of the legacy
/// <c>Mermer.Commerce.Models.Invoice</c>. All money fields are
/// <see cref="decimal"/>; no float/double anywhere.
///
/// Computed fields (subtotal, grand total, paid, left, …) are NOT stored
/// here — they're produced by the repository layer in SQL via
/// per-invoice aggregations and returned through <see cref="InvoiceInfo"/>.
/// </summary>
public class Invoice
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Sales;

    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? OfficeId { get; set; }
    public string? WarehouseId { get; set; }
    public string? DepositoryId { get; set; }
    public string? PartnerId { get; set; }
    public string? DisplayCurrencyId { get; set; }
    public string? StockPriceGroup { get; set; }

    /// <summary>
    /// If true, the unpaid remainder is moved to the partner debit/credit
    /// ledger instead of leaving the invoice "not paid".
    /// </summary>
    public bool DebitCreditLeftAmount { get; set; }

    public bool IsCompleted { get; set; }
    public bool IsDisabled { get; set; }
    public string? Group { get; set; }
    public List<string>? Tags { get; set; }
    public string? Description { get; set; }

    public List<InvoiceLine>     Lines     { get; set; } = new();
    public List<InvoiceDiscount> Discounts { get; set; } = new();
    public List<InvoicePayment>  Payments  { get; set; } = new();
    public List<InvoicePayment>  Changes   { get; set; } = new();
    public List<InvoiceOverhead> Overheads { get; set; } = new();
}

public class InvoiceLine
{
    public string? Id { get; set; }
    public string? SourceId { get; set; }
    public string? StockId { get; set; }
    public string? UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public string? CurrencyId { get; set; }
    public int SortOrder { get; set; }
}

public class InvoiceDiscount
{
    public string? Id { get; set; }
    public InvoiceDiscountType Type { get; set; } = InvoiceDiscountType.Flat;

    /// <summary>
    /// Absolute amount when <see cref="Type"/> is <see cref="InvoiceDiscountType.Flat"/>;
    /// percent value (0..100) when <see cref="InvoiceDiscountType.Percentage"/>.
    /// </summary>
    public decimal Amount { get; set; }

    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public class InvoicePayment
{
    public string? Id { get; set; }
    public decimal Amount { get; set; }
    public string? CurrencyId { get; set; }
    public int SortOrder { get; set; }
}

public class InvoiceOverhead
{
    public string? Id { get; set; }
    public decimal Amount { get; set; }
    public string? CurrencyId { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Aggregated invoice summary row — output of <c>GetInfoAsync</c>.
/// Financial totals are produced by per-child-table CTEs in SQL, with
/// correct Flat/Percentage discount semantics and overheads included.
/// </summary>
public class InvoiceInfo
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public DateTime Date { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDisabled { get; set; } // Для красного цвета в UI

    public string? PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public string? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public string? DepositoryId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Group { get; set; }
    public List<string>? Tags { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountsTotal { get; set; }
    public decimal OverheadsTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaymentsTotal { get; set; }
    public decimal LeftTotal { get; set; }
}

/// <summary>
/// Aggregated invoice summary including partner debit/credit ledger
/// (output of <c>GetPaymentInfoAsync</c>).
/// </summary>
public class InvoicePaymentInfo
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public DateTime Date { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public bool IsCompleted { get; set; }

    public string? PartnerId { get; set; }
    public string? PartnerName { get; set; }

    public decimal GrandTotal { get; set; }
    public decimal PaymentsTotal { get; set; }
    public decimal ChangesTotal { get; set; }
    public decimal PartnerDebit { get; set; }
    public decimal PartnerCredit { get; set; }
}
