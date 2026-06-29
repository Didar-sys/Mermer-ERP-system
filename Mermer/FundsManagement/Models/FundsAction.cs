// Decompiled with JetBrains decompiler
// Type: Mermer.FundsManagement.Models.FundsAction
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Mermer.FundsManagement.Models;

public class FundsAction
{
  public string TransactionId { get; set; }

  public string TransactionCode { get; set; }

  public DateTime TransactionDate { get; set; }

  public string TransactionType { get; set; }

  public string TransactionUserId { get; set; }

  public string TransactionUserName { get; set; }

  public bool TransactionIsCompleted { get; set; }

  public bool TransactionIsDisabled { get; set; }

  public string TransactionGroup { get; set; }

  public IEnumerable<string> TransactionTags { get; set; }

  public string ActionRelatedPartnerId { get; set; }

  public string ActionRelatedDepositoryId { get; set; }

  public string ActionRelatedObjectName { get; set; }

  public string ActionDepositoryId { get; set; }

  public string ActionCurrencyId { get; set; }

  public Decimal ActionAmount { get; set; }

  public Decimal ActionIncome { get; set; }

  public Decimal ActionExpense { get; set; }

  public Decimal ActionEffect => this.ActionIncome - this.ActionExpense;
    public decimal ActionEffectInCustomCurrency { get; set; }
}
