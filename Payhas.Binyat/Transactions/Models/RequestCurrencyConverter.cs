// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.RequestCurrencyConverter
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;
using System;

#nullable disable
namespace Payhas.Binyat.Transactions.Models;

public abstract class RequestCurrencyConverter : BindableObject, IRequestCurrencyConverter
{
  private CurrencyConvertion _displayCurrencyConvertion;

  public abstract void UpdateCurrencyConvertion();

  public event CurrencyConverter CurrencyConverterRequested;

  protected CurrencyConvertion GetCurrencyConvertion(string currencyId)
  {
    CurrencyConverter converterRequested = this.CurrencyConverterRequested;
    return converterRequested == null ? (CurrencyConvertion) null : converterRequested(currencyId);
  }

  public abstract void UpdateDisplayCurrencyId(bool raiseChangeEvent = false);

  public void UpdateDisplayCurrencyConvertion(CurrencyConvertion convertion, bool raiseChangeEvent = false)
  {
    this._displayCurrencyConvertion = convertion;
  }

  public event CurrencyId DisplayCurrencyIdRequested;

  protected string GetDisplayCurrencyId()
  {
    CurrencyId currencyIdRequested = this.DisplayCurrencyIdRequested;
    return currencyIdRequested == null ? (string) null : currencyIdRequested();
  }

  public abstract void UpdateDefaultCurrencyId();

  public event CurrencyId DefaultCurrencyIdRequested;

  protected string GetDefaultCurrencyId()
  {
    CurrencyId currencyIdRequested = this.DefaultCurrencyIdRequested;
    return currencyIdRequested == null ? (string) null : currencyIdRequested();
  }

  public event AmountFormatter AmountFormatterRequested;

  protected string GetAmountFormatter(Decimal amount, string currencyId)
  {
    AmountFormatter formatterRequested = this.AmountFormatterRequested;
    return formatterRequested == null ? (string) null : formatterRequested(amount, currencyId);
  }

  public Decimal GetDisplayAmount(Decimal amount)
  {
    if (this._displayCurrencyConvertion == null)
    {
      string displayCurrencyId = this.GetDisplayCurrencyId();
      if (string.IsNullOrEmpty(displayCurrencyId))
        return 0M;
      this._displayCurrencyConvertion = this.GetCurrencyConvertion(displayCurrencyId);
      if (this._displayCurrencyConvertion == null)
        return 0M;
    }
    return amount / this._displayCurrencyConvertion.Multiplier * this._displayCurrencyConvertion.Divider;
  }

  protected string GetDisplayAmountString(Decimal amount)
  {
    string displayCurrencyId = this.GetDisplayCurrencyId();
    return string.IsNullOrEmpty(displayCurrencyId) ? (string) null : this.GetAmountFormatter(amount, displayCurrencyId);
  }
}
