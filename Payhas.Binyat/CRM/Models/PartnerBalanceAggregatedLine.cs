// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.PartnerBalanceAggregatedLine
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;

#nullable disable
namespace Payhas.Binyat.CRM.Models;

public class PartnerBalanceAggregatedLine
{
  public PartnerBalanceAggregatedLine()
  {
  }

  public PartnerBalanceAggregatedLine(string type, Decimal effect)
    : this(type, effect > 0M ? effect : 0M, effect < 0M ? -effect : 0M)
  {
  }

  public PartnerBalanceAggregatedLine(string type, Decimal debit, Decimal credit)
  {
    this.Type = type;
    this.Debit = debit;
    this.Credit = credit;
  }

  public string Type { get; set; }

  public Decimal Debit { get; set; }

  public Decimal Credit { get; set; }

  public Decimal Effect => this.Debit - this.Credit;
}
