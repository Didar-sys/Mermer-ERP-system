using System;
using System.Collections.Generic;

namespace Mermer.Data.Postgres.Entities;

public class FundsSlipEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public DateTime Date { get; set; }
    public string FundsSlipType { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public Guid? OfficeId { get; set; }
    public Guid? DepositoryId { get; set; }
    public Guid? PartnerId { get; set; }
    public Guid? DisplayCurrencyId { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDisabled { get; set; }
    public string Group { get; set; }
    public string[] Tags { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserEntity User { get; set; }
    public OfficeEntity Office { get; set; }
    public DepositoryEntity Depository { get; set; }
    public PartnerEntity Partner { get; set; }
    public CurrencyEntity DisplayCurrency { get; set; }

    public ICollection<FundsSlipLineEntity> Lines { get; set; } = new List<FundsSlipLineEntity>();
}