using System;

namespace Mermer.Data.Postgres.Entities;

/// <summary>
/// Partner entity — customer or supplier.
/// Maps to: Payhas.Binyat.CRM.Models.Partner
/// </summary>
public class PartnerEntity
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Group { get; set; }

    /// <summary>NUMERIC(18,4) — maximum credit allowed.</summary>
    public decimal? CreditLimit { get; set; }

    public string[]? Tags { get; set; }
    public string? Description { get; set; }

    /// <summary>NUMERIC(18,4) — partner rating.</summary>
    public decimal Rating { get; set; }

    public Guid? CurrencyId { get; set; }
    public bool IsDisabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public CurrencyEntity? Currency { get; set; }
}
