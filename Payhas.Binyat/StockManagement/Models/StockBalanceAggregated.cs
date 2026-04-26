// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockBalanceAggregated
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockBalanceAggregated
{
  public StockBalanceAggregated()
  {
  }

  public StockBalanceAggregated(bool generateLines)
  {
    if (!generateLines)
      return;
    this.Lines = (IEnumerable<StockBalanceAggregatedLine>) new StockBalanceAggregatedLine[12]
    {
      new StockBalanceAggregatedLine("StockOpening", 0M),
      new StockBalanceAggregatedLine("StockSpoilage", 0M),
      new StockBalanceAggregatedLine("StockUsage", 0M),
      new StockBalanceAggregatedLine("RevisionExceed", 0M),
      new StockBalanceAggregatedLine("RevisionDeficit", 0M),
      new StockBalanceAggregatedLine("StockTransferSource", 0M),
      new StockBalanceAggregatedLine("StockTransferDestination", 0M),
      new StockBalanceAggregatedLine("Sales", 0M),
      new StockBalanceAggregatedLine("SalesReturn", 0M),
      new StockBalanceAggregatedLine("Purchase", 0M),
      new StockBalanceAggregatedLine("PurchaseReturn", 0M),
      new StockBalanceAggregatedLine("Repricing", 0M)
    };
  }

  public Decimal Income { get; set; }

  public Decimal Expense { get; set; }

  public Decimal EffectedBalance => this.Income - this.Expense;

  public Decimal StartingBalance { get; set; }

  public Decimal ResultingBalance => this.StartingBalance + this.EffectedBalance;

  public IEnumerable<StockBalanceAggregatedLine> Lines { get; set; }
}
