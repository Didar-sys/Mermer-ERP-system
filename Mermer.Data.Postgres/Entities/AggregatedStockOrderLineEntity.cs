using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mermer.Data.Postgres.Entities;

[Table("aggregated_stock_order_lines")]
public class AggregatedStockOrderLineEntity
{
    [Key][Column("id")] public Guid Id { get; set; }
    [Column("aggregated_stock_order_id")] public Guid AggregatedStockOrderId { get; set; }
    [Column("stock_id")] public Guid? StockId { get; set; }
    [Column("unit_id")] public Guid? UnitId { get; set; }

    [Column("orders", TypeName = "jsonb")]
    public string OrdersJson { get; set; } = "{}";

    [ForeignKey(nameof(AggregatedStockOrderId))]
    public AggregatedStockOrderEntity AggregatedStockOrder { get; set; } = null!;
}