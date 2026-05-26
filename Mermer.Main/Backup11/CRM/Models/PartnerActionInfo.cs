// Decompiled with JetBrains decompiler
// Type: Mermer.CRM.Models.PartnerActionInfo
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.CRM.Models;

public class PartnerActionInfo
{
  public string TransactionId { get; set; }

  public DateTime TransactionDate { get; set; }

  public Decimal ActionDebit { get; set; }

  public Decimal ActionCredit { get; set; }

  public Decimal ActionEffect => this.ActionDebit - this.ActionCredit;
}
