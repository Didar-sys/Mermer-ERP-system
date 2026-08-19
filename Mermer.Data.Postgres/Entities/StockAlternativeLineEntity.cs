using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mermer.Data.Postgres.Entities;

[Table("stock_alternative_lines")]
public class StockAlternativeLineEntity
{
    [Key][Column("id")] public Guid Id { get; set; }
    [Column("stock_alternative_id")] public Guid StockAlternativeId { get; set; }
    [Column("stock_id")] public Guid? StockId { get; set; }

    [ForeignKey(nameof(StockAlternativeId))]
    public StockAlternativeEntity Alternative { get; set; } = null!;
}