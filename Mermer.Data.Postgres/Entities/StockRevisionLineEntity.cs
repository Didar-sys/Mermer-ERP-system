using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mermer.Data.Postgres.Entities;

[Table("stock_revision_lines")]
public class StockRevisionLineEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("stock_revision_id")]
    public Guid StockRevisionId { get; set; }

    [Column("stock_id")]
    public Guid? StockId { get; set; }

    [Column("date")]
    public DateTimeOffset Date { get; set; }

    [Column("quantity")]
    public decimal Quantity { get; set; }

    [Column("unit_id")]
    public Guid? UnitId { get; set; }

    [Column("price")]
    public decimal? Price { get; set; }

    [Column("currency_id")]
    public Guid? CurrencyId { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("user_name")]
    public string? UserName { get; set; }

    [ForeignKey(nameof(StockRevisionId))]
    public StockRevisionEntity StockRevision { get; set; } = null!;
}