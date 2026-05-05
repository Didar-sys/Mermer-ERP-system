using System;
using System.Collections.Generic;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Currency entity.
/// Maps to: Mermer.FundsManagement.Models.Currency
/// </summary>
public class CurrencyEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Decimals { get; set; } = 2;
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
    public bool IsDisabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public ICollection<CurrencyRateEntity> Rates { get; set; } = new List<CurrencyRateEntity>();
}
