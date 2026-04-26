using System;

namespace Payhas.Binyat.Data.Postgres.Entities;

/// <summary>
/// Stock Balance entity — aggregated balance per warehouse/stock.
/// Maps to: Payhas.Binyat.StockManagement.Models.StockBalance
/// </summary>
public class StockBalanceEntity
{
    public Guid WarehouseId { get; set; }
    public Guid StockId { get; set; }

    /// <summary>NUMERIC(18,4) — total incoming quantity.</summary>
    public decimal Income { get; set; }

    /// <summary>NUMERIC(18,4) — total outgoing quantity.</summary>
    public decimal Expense { get; set; }

    /// <summary>Computed: Income - Expense</summary>
    public decimal Balance => Income - Expense;

    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public WarehouseEntity Warehouse { get; set; } = null!;
    public StockEntity Stock { get; set; } = null!;
}
