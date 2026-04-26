// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.FundsManagement.Models.FundsBalanceAggregated
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.FundsManagement.Models;

public class FundsBalanceAggregated
{
  public Decimal Income { get; set; }

  public Decimal Expense { get; set; }

  public Decimal EffectedBalance => this.Income - this.Expense;

  public Decimal StartingBalance { get; set; }

  public Decimal ResultingBalance => this.StartingBalance + this.EffectedBalance;

  public IEnumerable<FundsBalanceAggregatedLine> Lines { get; set; }
}
