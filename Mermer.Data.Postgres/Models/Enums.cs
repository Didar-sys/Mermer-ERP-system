namespace Mermer.Data.Postgres.Models;

/// <summary>
/// Type of an invoice — direction of stock and funds movement.
/// Mirrors the legacy <c>Payhas.Binyat.Commerce.Models.InvoiceType</c> exactly.
/// </summary>
public enum InvoiceType
{
    Purchase,
    PurchaseReturn,
    Sales,
    SalesReturn
}

/// <summary>
/// Discount semantics on invoice level.
/// Flat — absolute amount (in invoice currency).
/// Percentage — percent of the invoice line subtotal (0..100).
/// </summary>
public enum InvoiceDiscountType
{
    Flat,
    Percentage
}

/// <summary>
/// Invoice payment direction.
/// Payment — funds in (or out, depending on invoice type).
/// Change — change given back to the customer.
/// </summary>
public enum InvoicePaymentType
{
    Payment,
    Change
}

/// <summary>
/// Direction of a partner action (debit/credit ledger).
/// </summary>
public enum PartnerActionType
{
    Debit,
    Credit
}
