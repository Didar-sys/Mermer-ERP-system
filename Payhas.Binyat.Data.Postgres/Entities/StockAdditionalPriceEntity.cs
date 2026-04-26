using System;

namespace Payhas.Binyat.Data.Postgres.Entities;

/// <summary>
/// Stock Additional Price entity — alternate price list entry.
/// </summary>
public class StockAdditionalPriceEntity
{
    public Guid Id { get; set; }
    public Guid StockId { get; set; }

    /// <summary>NUMERIC(18,4)</summary>
    public decimal Price { get; set; }

    public Guid? CurrencyId { get; set; }
    public string? PriceGroup { get; set; }
    public DateTime ValidFrom { get; set; }

    // Navigation
    public StockEntity Stock { get; set; } = null!;
    public CurrencyEntity? Currency { get; set; }
}
