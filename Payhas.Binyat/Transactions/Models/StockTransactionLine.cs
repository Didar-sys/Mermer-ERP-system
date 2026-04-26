// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.StockTransactionLine
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Transactions.Models;

public abstract class StockTransactionLine : TransactionLine, IRequestStockUnitConverter
{
  private string _sourceId;
  private string _stockId;
  private Decimal _quantity;
  private string _unitId;
  private Decimal _price;

  protected StockTransactionLine()
  {
    this.AutoRaisePropertyChanged(new Dictionary<string, string[]>()
    {
      {
        "CurrencyId",
        new string[1]{ nameof (ActionPrice) }
      },
      {
        nameof (ActionQuantity),
        new string[1]{ nameof (ActionPrice) }
      },
      {
        nameof (ActionPrice),
        new string[1]{ nameof (ActionTotal) }
      },
      {
        nameof (DisplayPrice),
        new string[1]{ nameof (DisplayPriceString) }
      }
    });
  }

  public virtual string SourceId
  {
    get => this._sourceId;
    set => this.SetProperty<string>(ref this._sourceId, value, nameof (SourceId));
  }

  public virtual string StockId
  {
    get => this._stockId;
    set => this.SetProperty<string>(ref this._stockId, value, nameof (StockId), "ActionQuantity");
  }

  public virtual Decimal Quantity
  {
    get => this._quantity;
    set
    {
      this.SetProperty<Decimal>(ref this._quantity, value, nameof (Quantity), "ActionQuantity");
    }
  }

  public virtual string UnitId
  {
    get => this._unitId;
    set => this.SetProperty<string>(ref this._unitId, value, nameof (UnitId), "ActionQuantity");
  }

  public Decimal ActionQuantity
  {
    get
    {
      if (string.IsNullOrEmpty(this.StockId) || string.IsNullOrEmpty(this.UnitId))
        return 0M;
      StockUnitConvertion stockUnitConvertion = this.GetStockUnitConvertion(this.StockId, this.UnitId);
      return stockUnitConvertion == null ? 0M : this.Quantity * stockUnitConvertion.Multiplier / stockUnitConvertion.Divider;
    }
  }

  public virtual Decimal Price
  {
    get => this._price;
    set => this.SetProperty<Decimal>(ref this._price, value, nameof (Price), "ActionPrice");
  }

  public Decimal ActionPrice
  {
    get
    {
      if (string.IsNullOrEmpty(this.StockId) || string.IsNullOrEmpty(this.UnitId) || string.IsNullOrEmpty(this.CurrencyId))
        return 0M;
      StockUnitConvertion stockUnitConvertion = this.GetStockUnitConvertion(this.StockId, this.UnitId);
      if (stockUnitConvertion == null)
        return 0M;
      CurrencyConvertion currencyConvertion = this.GetCurrencyConvertion(this.CurrencyId);
      return currencyConvertion == null ? 0M : this.Price * currencyConvertion.Multiplier / currencyConvertion.Divider * stockUnitConvertion.Divider / stockUnitConvertion.Multiplier;
    }
  }

  public override Decimal ActionTotal => this.ActionPrice * this.ActionQuantity;

  public virtual Decimal DisplayPrice => this.GetDisplayAmount(this.ActionPrice);

  public virtual string DisplayPriceString => this.GetDisplayAmountString(this.DisplayPrice);

  public override void UpdateCurrencyConvertion()
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionPrice));
  }

  public virtual void UpdateStockUnitConvertion()
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionQuantity));
  }

  public override void UpdateDisplayCurrencyId(bool raiseChangeEvent = false)
  {
    base.UpdateDisplayCurrencyId(raiseChangeEvent);
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayPrice));
  }

  public event StockUnitConverter StockUnitConverterRequested;

  protected StockUnitConvertion GetStockUnitConvertion(string stockId, string unitId)
  {
    StockUnitConverter converterRequested = this.StockUnitConverterRequested;
    return converterRequested == null ? (StockUnitConvertion) null : converterRequested(stockId, unitId);
  }
}
