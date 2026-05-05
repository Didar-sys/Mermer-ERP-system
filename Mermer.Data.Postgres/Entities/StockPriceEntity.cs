using System;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Stock Price entity — historical price entry for a product.
/// Maps to: Mermer.StockManagement.Models.StockPrice
/// </summary>
public class StockPriceEntity
{
    public Guid Id { get; set; }
    public Guid StockId { get; set; }
    public DateTime ValidFrom { get; set; }

    /// <summary>NUMERIC(18,4) — product price. Never float/double.</summary>
    public decimal Price { get; set; }

    public Guid? CurrencyId { get; set; }
    public string? PriceGroup { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public StockEntity Stock { get; set; } = null!;
    public CurrencyEntity? Currency { get; set; }
}
