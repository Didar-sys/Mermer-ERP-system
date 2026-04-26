using System;
using System.Collections.Generic;

namespace Payhas.Binyat.Data.Postgres.Entities;

/// <summary>
/// Office entity — represents a branch/office location.
/// Maps to: Payhas.Binyat.Enterprise.Models.Office
/// </summary>
public class OfficeEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
    public bool IsDisabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public ICollection<WarehouseEntity> Warehouses { get; set; } = new List<WarehouseEntity>();
    public ICollection<DepositoryEntity> Depositories { get; set; } = new List<DepositoryEntity>();
}
