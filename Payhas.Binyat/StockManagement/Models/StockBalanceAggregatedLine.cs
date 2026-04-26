// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockBalanceAggregatedLine
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockBalanceAggregatedLine
{
  public StockBalanceAggregatedLine()
  {
  }

  public StockBalanceAggregatedLine(string type, Decimal effect)
    : this(type, effect > 0M ? effect : 0M, effect < 0M ? -effect : 0M)
  {
  }

  public StockBalanceAggregatedLine(string type, Decimal income, Decimal expense)
  {
    this.Type = type;
    this.Income = income;
    this.Expense = expense;
  }

  public string Type { get; set; }

  public Decimal Income { get; set; }

  public Decimal Expense { get; set; }

  public Decimal Effect => this.Income - this.Expense;
}
