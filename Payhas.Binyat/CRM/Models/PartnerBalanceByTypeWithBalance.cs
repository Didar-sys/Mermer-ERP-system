// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.PartnerBalanceByTypeWithBalance
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;

#nullable disable
namespace Payhas.Binyat.CRM.Models;

public class PartnerBalanceByTypeWithBalance : PartnerBalanceByType
{
  public Decimal StartingBalance { get; set; }

  public Decimal ResultingBalance => this.StartingBalance + this.Balance;

  public Decimal ResultingBalanceInCustomCurrency { get; set; }
}
