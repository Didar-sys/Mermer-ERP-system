using System;

namespace Payhas.Binyat.Data.Postgres.Entities;

/// <summary>
/// Depository entity — cash register or fund storage.
/// Maps to: Payhas.Binyat.Enterprise.Models.Depository
/// </summary>
public class DepositoryEntity
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
