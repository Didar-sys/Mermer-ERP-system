using System;

namespace Payhas.Binyat.Data.Postgres.Entities;

/// <summary>
/// Invoice Currency Convertion entity — snapshot of exchange rate at invoice time.
/// Maps to: Payhas.Binyat.Transactions.Models.CurrencyConvertion
/// </summary>
public class InvoiceCurrencyConvertionEntity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid CurrencyId { get; set; }

    /// <summary>NUMERIC(18,8)</summary>
    public decimal Multiplier { get; set; } = 1m;

    /// <summary>NUMERIC(18,8)</summary>
    public decimal Divider { get; set; } = 1m;

    // Navigation
    public InvoiceEntity Invoice { get; set; } = null!;
    public CurrencyEntity Currency { get; set; } = null!;
}
