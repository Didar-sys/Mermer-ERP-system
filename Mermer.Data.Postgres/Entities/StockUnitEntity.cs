using System;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Stock Unit entity — unit of measurement for a product (e.g., pcs, kg, box).
/// Maps to: Mermer.StockManagement.Models.StockUnit
/// </summary>
public class StockUnitEntity
{
    public Guid Id { get; set; }
    public Guid StockId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>NUMERIC(18,8) — conversion multiplier. Must not be zero.</summary>
    public decimal Multiplier { get; set; } = 1m;

    /// <summary>NUMERIC(18,8) — conversion divider. Must not be zero.</summary>
    public decimal Divider { get; set; } = 1m;

    public bool IsDefault { get; set; }

    // Navigation
    public StockEntity Stock { get; set; } = null!;
}
