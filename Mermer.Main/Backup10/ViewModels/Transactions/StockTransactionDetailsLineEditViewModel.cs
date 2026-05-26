// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Transactions.StockTransactionDetailsLineEditViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.StockManagement.Models;
using Mermer.Data.Models;
using Mermer.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Transactions;

public class StockTransactionDetailsLineEditViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  TransactionLineEditViewModel<StockTransactionDetailsLineEditViewModel.Params, StockTransactionDetailsLineEditViewModel.Result>(messenger, navigationService, userInteractionService)
{
  public override void Prepare(
    StockTransactionDetailsLineEditViewModel.Params parameter)
  {
    base.Prepare(parameter);
    parameter.PriceChanged = false;
  }

  public class Params : StockTransactionDetailsLineEditViewModel.Result
  {
    public string StockCode { get; set; }

    public string StockName { get; set; }

    public DateTime? ActionDate { get; set; }

    public IEnumerable<StockUnit> Units { get; set; }

    public IEnumerable<Currency> Currencies { get; set; }

    protected override void CalculatePriceOnUnitChange(string prevUnitId)
    {
      if (prevUnitId == this.UnitId)
        return;
      StockUnit stockUnit1 = this.Units.SingleOrDefault<StockUnit>((Func<StockUnit, bool>) (x => x.Id == prevUnitId));
      StockUnit stockUnit2 = this.Units.SingleOrDefault<StockUnit>((Func<StockUnit, bool>) (x => x.Id == this.UnitId));
      if (stockUnit1 != null && stockUnit2 != null)
        this.Price = this.Price * stockUnit1.Divider / stockUnit1.Multiplier * stockUnit2.Multiplier / stockUnit2.Divider;
      this.PriceChanged = false;
    }

    protected override void CalculatePriceOnCurrencyChange(string prevCurrencyId)
    {
      if (!this.ActionDate.HasValue || prevCurrencyId == this.CurrencyId)
        return;
      Currency currency1 = this.Currencies.SingleOrDefault<Currency>((Func<Currency, bool>) (x => x.Id == prevCurrencyId));
      Currency currency2 = this.Currencies.SingleOrDefault<Currency>((Func<Currency, bool>) (x => x.Id == this.CurrencyId));
      if (currency1 != null && currency2 != null)
      {
        CurrencyRate rate1 = currency1.GetRate(this.ActionDate);
        CurrencyRate rate2 = currency2.GetRate(this.ActionDate);
        this.Price = Math.Round(this.Price * rate1.Multiplier / rate1.Divider * rate2.Divider / rate2.Multiplier, currency2.Decimals);
      }
      this.PriceChanged = false;
    }
  }

  public class Result : BindableObject
  {
    public bool PriceChanged;
    private string _unitId;
    private Decimal _price;
    private string _currencyId;

    public Decimal Quantity { get; set; }

    public string UnitId
    {
      get => this._unitId;
      set
      {
        string unitId = this._unitId;
        this.SetProperty<string>(ref this._unitId, value, nameof (UnitId));
        if (this.PriceChanged || string.IsNullOrEmpty(unitId))
          return;
        this.CalculatePriceOnUnitChange(unitId);
      }
    }

    public Decimal Price
    {
      get => this._price;
      set
      {
        this.SetProperty<Decimal>(ref this._price, value, nameof (Price));
        this.PriceChanged = true;
      }
    }

    public string CurrencyId
    {
      get => this._currencyId;
      set
      {
        string currencyId = this._currencyId;
        this.SetProperty<string>(ref this._currencyId, value, nameof (CurrencyId));
        if (this.PriceChanged || string.IsNullOrEmpty(currencyId))
          return;
        this.CalculatePriceOnCurrencyChange(currencyId);
      }
    }

    protected virtual void CalculatePriceOnUnitChange(string prevUnitId)
    {
    }

    protected virtual void CalculatePriceOnCurrencyChange(string prevCurrencyId)
    {
    }
  }
}
