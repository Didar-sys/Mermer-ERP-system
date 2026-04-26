using System;

namespace Payhas.Binyat.Data.Postgres.Entities;

/// <summary>
/// Invoice Stock Unit Convertion entity — snapshot of unit conversion at invoice time.
/// Maps to: Payhas.Binyat.Transactions.Models.StockUnitConvertion
/// </summary>
public class InvoiceStockUnitConvertionEntity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid StockId { get; set; }
    public Guid UnitId { get; set; }

    /// <summary>NUMERIC(18,8)</summary>
    public decimal Multiplier { get; set; } = 1m;

    /// <summary>NUMERIC(18,8)</summary>
    public decimal Divider { get; set; } = 1m;

    // Navigation
    public InvoiceEntity Invoice { get; set; } = null!;
    public StockEntity Stock { get; set; } = null!;
    public StockUnitEntity Unit { get; set; } = null!;
}
