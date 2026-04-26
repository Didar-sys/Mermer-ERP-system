// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Revisioning.Models.StockRevisionUncountedInfo
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Warehousing.Revisioning.Models;

public class StockRevisionUncountedInfo : BindableObject
{
  private Decimal _counted;

  public string StockRevisionId { get; set; }

  public string StockId { get; set; }

  public string StockCode { get; set; }

  public string StockName { get; set; }

  public string StockUnit { get; set; }

  public string StockUnitId { get; set; }

  public Decimal Computed { get; set; }

  public Decimal Counted
  {
    get => this._counted;
    set
    {
      if (!this.SetProperty<Decimal>(ref this._counted, value, nameof (Counted)))
        return;
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.Difference));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsExceed));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsDeficit));
    }
  }

  public Decimal Difference => this.Counted - this.Computed;

  public bool IsExceed => this.Difference > 0M;

  public bool IsDeficit => this.Difference < 0M;
}
