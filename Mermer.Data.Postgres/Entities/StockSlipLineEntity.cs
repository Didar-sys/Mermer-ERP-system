using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mermer.Data.Postgres.Entities;

[Table("stock_slip_lines")] // Изменено на snake_case
public class StockSlipLineEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid StockSlipId { get; set; }
    public Guid? StockId { get; set; }
    public Guid? UnitId { get; set; }

    public decimal Quantity { get; set; }
    public decimal ActionQuantity { get; set; }
    public decimal Price { get; set; }
    public decimal ActionTotal { get; set; }
    public int SortOrder { get; set; }

    // ── Добавленные навигационные свойства ──
    public StockEntity? Stock { get; set; }
    public StockUnitEntity? Unit { get; set; }

    // Навигационное свойство обратно к ордеру
    [ForeignKey(nameof(StockSlipId))]
    public StockSlipEntity StockSlip { get; set; } = null!;
}