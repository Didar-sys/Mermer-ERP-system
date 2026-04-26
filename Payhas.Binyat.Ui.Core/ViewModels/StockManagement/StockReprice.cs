// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.StockReprice
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.Transactions.Models;
using System;
using System.ComponentModel;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement;

public class StockReprice : RequestCurrencyConverter
{
  private Stock _stock;
  private Decimal _currentPrice;
  private string _currentPriceCurrencyId;
  private Decimal _referencePrice;
  private string _referencePriceCurrencyId;

  public virtual Stock Stock
  {
    get => this._stock;
    set
    {
      if (this._stock != null)
        this._stock.PropertyChanged -= new PropertyChangedEventHandler(this.Stock_PropertyChanged);
      this.SetProperty<Stock>(ref this._stock, value, nameof (Stock));
      if (this._stock != null)
        this._stock.PropertyChanged += new PropertyChangedEventHandler(this.Stock_PropertyChanged);
      Stock stock = this._stock;
      this.CurrentPrice = stock != null ? stock.Price : 0M;
      this.CurrentPriceCurrencyId = this._stock?.CurrencyId;
    }
  }

  private void Stock_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!(e.PropertyName == "Price") && !(e.PropertyName == "CurrencyId"))
      return;
    this.RaisePropertyChanged("NewPriceValue");
  }

  public virtual Decimal CurrentPrice
  {
    get => this._currentPrice;
    set
    {
      this.SetProperty<Decimal>(ref this._currentPrice, value, nameof (CurrentPrice), "CurrentPriceValue");
    }
  }

  public virtual string CurrentPriceCurrencyId
  {
    get => this._currentPriceCurrencyId;
    set
    {
      this.SetProperty<string>(ref this._currentPriceCurrencyId, value, nameof (CurrentPriceCurrencyId), "CurrentPriceValue");
    }
  }

  public virtual Decimal ReferencePrice
  {
    get => this._referencePrice;
    set
    {
      this.SetProperty<Decimal>(ref this._referencePrice, value, nameof (ReferencePrice), "ReferencePriceValue");
    }
  }

  public virtual string ReferencePriceCurrencyId
  {
    get => this._referencePriceCurrencyId;
    set
    {
      this.SetProperty<string>(ref this._referencePriceCurrencyId, value, nameof (ReferencePriceCurrencyId), "ReferencePriceValue");
    }
  }

  public Decimal NewPriceValue
  {
    get
    {
      if (string.IsNullOrEmpty(this.Stock?.CurrencyId))
        return 0M;
      CurrencyConvertion currencyConvertion = this.GetCurrencyConvertion(this.Stock.CurrencyId);
      return currencyConvertion == null ? 0M : this.Stock.Price * currencyConvertion.Multiplier / currencyConvertion.Divider;
    }
  }

  public Decimal CurrentPriceValue
  {
    get
    {
      if (string.IsNullOrEmpty(this.CurrentPriceCurrencyId))
        return 0M;
      CurrencyConvertion currencyConvertion = this.GetCurrencyConvertion(this.CurrentPriceCurrencyId);
      return currencyConvertion == null ? 0M : this.CurrentPrice * currencyConvertion.Multiplier / currencyConvertion.Divider;
    }
  }

  public Decimal ReferencePriceValue
  {
    get
    {
      if (string.IsNullOrEmpty(this.ReferencePriceCurrencyId))
        return 0M;
      CurrencyConvertion currencyConvertion = this.GetCurrencyConvertion(this.ReferencePriceCurrencyId);
      return currencyConvertion == null ? 0M : this.ReferencePrice * currencyConvertion.Multiplier / currencyConvertion.Divider;
    }
  }

  public override void UpdateCurrencyConvertion()
  {
    this.RaisePropertyChanged("NewPriceValue");
    this.RaisePropertyChanged("CurrentPriceValue");
    this.RaisePropertyChanged("ReferencePriceValue");
  }

  public override void UpdateDefaultCurrencyId() => throw new NotImplementedException();

  public override void UpdateDisplayCurrencyId(bool raiseChangeEvent = false)
  {
    throw new NotImplementedException();
  }
}
