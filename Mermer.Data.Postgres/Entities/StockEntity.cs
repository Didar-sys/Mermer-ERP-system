using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Stock entity — product/item in inventory.
/// Maps to: Payhas.Binyat.StockManagement.Models.Stock
/// </summary>
public class StockEntity
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Type { get; set; }
    public string? Group { get; set; }
    public string[]? Tags { get; set; }
    public string[]? Barcodes { get; set; }

    /// <summary>NUMERIC(18,4) — minimum stock alert level.</summary>
    public decimal? LimitMin { get; set; }

    /// <summary>NUMERIC(18,4) — maximum stock alert level.</summary>
    public decimal? LimitMax { get; set; }

    public string? Description { get; set; }
    public bool IsDisabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Auto-generated TSVECTOR column for full-text search.</summary>
    public NpgsqlTsVector? SearchVector { get; set; }

    // Navigation
    public ICollection<StockUnitEntity> Units { get; set; } = new List<StockUnitEntity>();
    public ICollection<StockPriceEntity> Prices { get; set; } = new List<StockPriceEntity>();
    public ICollection<StockAdditionalPriceEntity> AdditionalPrices { get; set; } = new List<StockAdditionalPriceEntity>();
}
