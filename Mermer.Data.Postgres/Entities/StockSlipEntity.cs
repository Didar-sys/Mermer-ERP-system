using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mermer.Data.Postgres.Entities;

[Table("stock_slips")] // Изменено на snake_case
public class StockSlipEntity
{
    [Key]
    public Guid Id { get; set; }

    public string? Code { get; set; }
    public string SlipType { get; set; } = "StockOpening";
    public bool IsCompleted { get; set; }
    public bool IsStockIncome { get; set; }
    public decimal DisplayTotal { get; set; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }

    public DateTimeOffset Date { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? UserId { get; set; }
    public Guid? WarehouseId { get; set; }

    // ── Добавленные навигационные свойства ──
    public UserEntity? User { get; set; }
    public WarehouseEntity? Warehouse { get; set; }

    // Навигационное свойство для связи со строками
    public ICollection<StockSlipLineEntity> Lines { get; set; } = new List<StockSlipLineEntity>();
}