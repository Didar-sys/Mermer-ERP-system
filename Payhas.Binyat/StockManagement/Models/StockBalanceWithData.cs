// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockBalanceWithData
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockBalanceWithData : StockBalance
{
  public string StockCode { get; set; }

  public string StockName { get; set; }

  public string StockUnitId { get; set; }

  public string StockUnit { get; set; }

  public Decimal StockPrice { get; set; }

  public string StockType { get; set; }

  public string StockGroup { get; set; }

  public IEnumerable<string> StockTags { get; set; }

  public bool IsExisting { get; set; }

  public bool IsFinished { get; set; }

  public bool IsOverUsed { get; set; }

  public bool IsFinishing { get; set; }

  public bool IsOverLimit { get; set; }

  public Decimal Total => this.Balance * this.StockPrice;
}
