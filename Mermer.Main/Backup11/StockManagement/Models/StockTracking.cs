// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.StockTracking
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.StockManagement.Models;

public class StockTracking
{
  public string WarehouseId { get; set; }

  public string StockId { get; set; }

  public string StockCode { get; set; }

  public string StockName { get; set; }

  public Decimal Income { get; set; }

  public Decimal Expense { get; set; }

  public Decimal Left => this.Income - this.Expense;
}
