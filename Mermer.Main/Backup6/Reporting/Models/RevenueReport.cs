// Decompiled with JetBrains decompiler
// Type: Mermer.Reporting.Models.RevenueReport
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Mermer.Reporting.Models;

public class RevenueReport
{
  public DateTime Date { get; set; }

  public string WarehouseId { get; set; }

  public string StockId { get; set; }

  public string StockCode { get; set; }

  public string StockName { get; set; }

  public string StockUnit { get; set; }

  public string StockType { get; set; }

  public string StockGroup { get; set; }

  public IEnumerable<string> StockTags { get; set; }

  public Decimal Quantity { get; set; }

  public Decimal InitialCosts { get; set; }

  public Decimal OverheadsCosts { get; set; }

  public Decimal CostsTotal => this.InitialCosts + this.OverheadsCosts;

  public Decimal RecommendedPrice { get; set; }

  public Decimal RecommendedTotal => this.Quantity * this.RecommendedPrice;

  public Decimal RecommendedEarning => this.RecommendedTotal - this.CostsTotal;

  public Decimal ActualPrice { get; set; }

  public Decimal ActualTotal => this.Quantity * this.ActualPrice;

  public Decimal ActualEarning => this.ActualTotal - this.CostsTotal;
}
