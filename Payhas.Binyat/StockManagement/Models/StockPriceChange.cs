// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockPriceChange
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockPriceChange
{
  public string StockId { get; set; }

  public string StockCode { get; set; }

  public string StockName { get; set; }

  public DateTime ChangeDate { get; set; }

  public Decimal PriceChange { get; set; }

  public StockPriceChangeReason ChangeReason { get; set; }
}
