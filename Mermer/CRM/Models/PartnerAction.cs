// Decompiled with JetBrains decompiler
// Type: Mermer.CRM.Models.PartnerAction
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Mermer.CRM.Models;

public class PartnerAction
{
    public decimal ActionEffectInCustomCurrency { get; set; }
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

  public string ActionOfficeId { get; set; }

  public string ActionPartnerId { get; set; }

  public Decimal ActionDebit { get; set; }

  public Decimal ActionCredit { get; set; }

  public Decimal ActionEffect => this.ActionDebit - this.ActionCredit;
}
