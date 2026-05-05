using System;
using System.Collections.Generic;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Warehouse entity — physical storage location.
/// Maps to: Mermer.Enterprise.Models.Warehouse
/// </summary>
public class WarehouseEntity
{
    public Guid Id { get; set; }
    public Guid? OfficeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
    public bool IsDisabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public OfficeEntity? Office { get; set; }
}
