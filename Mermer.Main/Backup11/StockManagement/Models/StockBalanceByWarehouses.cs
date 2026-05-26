// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.StockBalanceByWarehouses
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Mermer.StockManagement.Models;

public class StockBalanceByWarehouses
{
  public string StockId { get; set; }

  public string StockCode { get; set; }

  public string StockName { get; set; }

  public string StockUnit { get; set; }

  public Decimal StockPrice { get; set; }

  public string StockPriceCurrencyId { get; set; }

  public string StockGroup { get; set; }

  public string StockType { get; set; }

  public string StockTags { get; set; }

  public Dictionary<string, Decimal> Balances { get; set; }

  public Decimal TotalBalance
  {
    get
    {
      Dictionary<string, Decimal> balances = this.Balances;
      return balances == null ? 0M : balances.Sum<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, Decimal>) (x => x.Value));
    }
  }

  public Decimal TotalAmount => this.TotalBalance * this.StockPrice;
}
