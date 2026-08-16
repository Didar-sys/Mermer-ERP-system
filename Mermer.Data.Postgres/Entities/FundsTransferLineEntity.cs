using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mermer.Data.Postgres.Entities;

public class FundsTransferLineEntity
{
    public Guid Id { get; set; }
    public Guid FundsTransferId { get; set; }
    public decimal Amount { get; set; }

    // Явно говорим EF Core искать колонку "received_amount" в нижнем регистре
    [Column("received_amount")]
    public decimal ReceivedAmount { get; set; }

    public Guid? CurrencyId { get; set; }
    public int SortOrder { get; set; }

    public FundsTransferEntity FundsTransfer { get; set; }
    public CurrencyEntity Currency { get; set; }
}