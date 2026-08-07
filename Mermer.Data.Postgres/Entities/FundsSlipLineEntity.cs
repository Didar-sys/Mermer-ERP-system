using System;

namespace Mermer.Data.Postgres.Entities;

public class FundsSlipLineEntity
{
    public Guid Id { get; set; }
    public Guid FundsSlipId { get; set; }
    public decimal Amount { get; set; }
    public Guid? CurrencyId { get; set; }
    public int SortOrder { get; set; }

    public FundsSlipEntity FundsSlip { get; set; }
    public CurrencyEntity Currency { get; set; }
}