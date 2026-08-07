using System;
using System.Collections.Generic;

namespace Mermer.Data.Postgres.Entities;

public class FundsTransferEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public DateTime Date { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public Guid? FromDepositoryId { get; set; }
    public Guid? ToDepositoryId { get; set; }
    public Guid? DisplayCurrencyId { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDisabled { get; set; }
    public string Group { get; set; }
    public string[] Tags { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserEntity User { get; set; }
    public DepositoryEntity FromDepository { get; set; }
    public DepositoryEntity ToDepository { get; set; }
    public CurrencyEntity DisplayCurrency { get; set; }

    public ICollection<FundsTransferLineEntity> Lines { get; set; } = new List<FundsTransferLineEntity>();
}