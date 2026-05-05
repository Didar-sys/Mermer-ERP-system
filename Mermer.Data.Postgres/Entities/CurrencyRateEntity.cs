using System;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Currency Rate entity — historical exchange rate entry.
/// Maps to: Mermer.FundsManagement.Models.CurrencyRate
/// </summary>
public class CurrencyRateEntity
{
    public Guid Id { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime ValidFrom { get; set; }

    /// <summary>NUMERIC(18,8) — multiplier for currency conversion. Must not be zero.</summary>
    public decimal Multiplier { get; set; } = 1m;

    /// <summary>NUMERIC(18,8) — divider for currency conversion. Must not be zero.</summary>
    public decimal Divider { get; set; } = 1m;

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public CurrencyEntity Currency { get; set; } = null!;
}
