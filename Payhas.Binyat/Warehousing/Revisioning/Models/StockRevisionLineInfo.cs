// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Revisioning.Models.StockRevisionLineInfo
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Transactions.Models;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Warehousing.Revisioning.Models;

public class StockRevisionLineInfo : RequestCurrencyConverter
{
  public string StockRevisionId { get; set; }

  public string StockRevisionLineId { get; set; }

  public string UserId { get; set; }

  public string UserName { get; set; }

  public string StockId { get; set; }

  public string StockCode { get; set; }

  public string StockName { get; set; }

  public Decimal StockPrice { get; set; }

  public string StockPriceCurrencyId { get; set; }

  public DateTime Date { get; set; }

  public Decimal Quantity { get; set; }

  public string UnitId { get; set; }

  public string Unit { get; set; }

  public Decimal CurrentCounted { get; set; }

  public Decimal TotalCounted { get; set; }

  public Decimal TotalComputed { get; set; }

  public Decimal TotalDifference => Math.Round(this.TotalCounted - this.TotalComputed, 2);

  public Decimal Total => this.TotalDifference * this.StockPrice;

  public Decimal ActionTotal
  {
    get
    {
      if (string.IsNullOrEmpty(this.StockPriceCurrencyId))
        return 0M;
      CurrencyConvertion currencyConvertion = this.GetCurrencyConvertion(this.StockPriceCurrencyId);
      return currencyConvertion == null ? 0M : this.Total * currencyConvertion.Multiplier / currencyConvertion.Divider;
    }
  }

  public virtual Decimal DisplayTotal => this.GetDisplayAmount(this.ActionTotal);

  public bool IsExceed => this.TotalDifference > 0M;

  public bool IsDeficit => this.TotalDifference < 0M;

  public override void UpdateCurrencyConvertion()
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionTotal));
  }

  public override void UpdateDefaultCurrencyId()
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayTotal));
  }

  public override void UpdateDisplayCurrencyId(bool raiseChangeEvent = false)
  {
  }
}
