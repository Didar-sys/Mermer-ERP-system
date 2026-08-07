using System;

namespace Mermer.Data.Postgres.Entities;

public class FundsTransferLineEntity
{
    public Guid Id { get; set; }
    public Guid FundsTransferId { get; set; }
    public decimal Amount { get; set; }
    public Guid? CurrencyId { get; set; }
    public int SortOrder { get; set; }

    public FundsTransferEntity FundsTransfer { get; set; }
    public CurrencyEntity Currency { get; set; }
}