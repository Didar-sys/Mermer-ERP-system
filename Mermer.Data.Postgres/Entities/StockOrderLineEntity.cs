using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mermer.Data.Postgres.Entities;

[Table("stock_order_lines")]
public class StockOrderLineEntity
{
    [Key][Column("id")] public Guid Id { get; set; }
    [Column("stock_order_id")] public Guid StockOrderId { get; set; }
    [Column("stock_id")] public Guid? StockId { get; set; }
    [Column("quantity")] public decimal Quantity { get; set; }
    [Column("unit_id")] public Guid? UnitId { get; set; }

    [ForeignKey(nameof(StockOrderId))]
    public StockOrderEntity StockOrder { get; set; } = null!;
}