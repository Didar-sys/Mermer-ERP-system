// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning.StockRevisionDetailsLineEditViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Data.Models;
using Payhas.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning;

public class StockRevisionDetailsLineEditViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  TransactionLineEditViewModel<StockRevisionDetailsLineEditViewModel.Params, StockRevisionDetailsLineEditViewModel.Result>(messenger, navigationService, userInteractionService)
{
  public class Params : StockRevisionDetailsLineEditViewModel.Result
  {
    public string StockCode { get; set; }

    public string StockName { get; set; }

    public bool IsPriceReadonly { get; set; }

    public IEnumerable<Currency> Currencies { get; set; }

    public IEnumerable<StockUnit> Units { get; set; }

    public Decimal Counted
    {
      get
      {
        if (string.IsNullOrEmpty(this.UnitId))
          return 0M;
        StockUnit stockUnit = this.Units.Single<StockUnit>((Func<StockUnit, bool>) (x => x.Id == this.UnitId));
        return this.Quantity * stockUnit.Multiplier / stockUnit.Divider;
      }
    }

    public Decimal PreviousCounted { get; set; }

    public Decimal TotalComputed { get; set; }

    public Decimal TotalDifference => this.Counted + this.PreviousCounted - this.TotalComputed;

    public StockUnit TotalUnit
    {
      get => this.Units.Single<StockUnit>((Func<StockUnit, bool>) (x => x.IsDefault));
    }
  }

  public class Result : BindableObject
  {
    private Decimal _quantity;
    private string _unitId;
    private Decimal _price;
    private string _currencyId;

    public Decimal Quantity
    {
      get => this._quantity;
      set
      {
        if (!this.SetProperty<Decimal>(ref this._quantity, value, nameof (Quantity)))
          return;
        this.RaisePropertyChanged("Counted");
        this.RaisePropertyChanged("TotalDifference");
      }
    }

    public string UnitId
    {
      get => this._unitId;
      set
      {
        if (!this.SetProperty<string>(ref this._unitId, value, nameof (UnitId)))
          return;
        this.RaisePropertyChanged("Counted");
        this.RaisePropertyChanged("TotalDifference");
      }
    }

    public Decimal Price
    {
      get => this._price;
      set => this.SetProperty<Decimal>(ref this._price, value, nameof (Price));
    }

    public string CurrencyId
    {
      get => this._currencyId;
      set => this.SetProperty<string>(ref this._currencyId, value, nameof (CurrencyId));
    }
  }
}
