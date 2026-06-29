// Decompiled with JetBrains decompiler
// Type: Mermer.FundsManagement.Models.FundsBalanceByTypeWithBalance
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.FundsManagement.Models;

public class FundsBalanceByTypeWithBalance : FundsBalanceByType
{

  public Decimal StartingBalance { get; set; }

  public Decimal ResultingBalance => this.StartingBalance + this.Balance;
    public decimal ResultingBalanceInCustomCurrency { get; set; }
}
