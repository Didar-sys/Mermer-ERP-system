// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.StockSlipDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.Services;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Binyat.Warehousing.Models;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing;

public class StockSlipDetailsViewModel : 
  StockTransactionDetailsViewModel<StockSlip, StockSlipLine, StockSlipType>,
  IMvxViewModel<StockSlipType>,
  IMvxViewModel
{
  private StockSlipType _newSlipType = StockSlipType.StockUsage;
  private readonly IPrintingService _printingService;

  public StockSlipDetailsViewModel(
    CopyCreate copyCreate,
    IConfigurator configurator,
    ILoginService loginService,
    StockSearcher stockSearcher,
    Reference<Currency> currencies,
    Reference<Warehouse> warehouses,
    IPrintingService printingService,
    IRepository<StockSlip> repository,
    IListAuthorizer<StockSlip> authorizer,
    IStocksRepository stocksRepository,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IUserInteractionService userInteractionService)
    : base(copyCreate, repository, authorizer, configurator, loginService, stockSearcher, currencies, warehouses, stocksRepository, navigationService, codegentor, userInteractionService)
  {
    this._printingService = printingService;
  }

  public void Prepare(StockSlipType parameter) => this._newSlipType = parameter;

  protected override async Task PostLoad()
  {
    StockSlipDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (!string.IsNullOrEmpty(detailsViewModel.ItemId))
      return;
    detailsViewModel.Details.SlipType = detailsViewModel._newSlipType;
  }

  protected override async Task<bool> OnSaveAsync()
  {
    StockSlipDetailsViewModel detailsViewModel = this;
    detailsViewModel._newSlipType = detailsViewModel.Details.SlipType;
    // ISSUE: reference to a compiler-generated method
    if (!await detailsViewModel.\u003C\u003En__1())
      return false;
    await detailsViewModel._printingService.PrintStockSlip(detailsViewModel.Details);
    return true;
  }

  protected override void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    base.Details_PropertyChanged(sender, e);
    if (!(e.PropertyName == "IsPriceEditable") || this.Details.IsPriceEditable)
      return;
    foreach (StockSlipLine line in (Collection<StockSlipLine>) this.Details.Lines)
    {
      Stock fromStocksCache = this.GetFromStocksCache(line.StockId);
      Decimal num = line.ActionQuantity / line.Quantity;
      DateTime? date = new DateTime?(this.Details.Date);
      StockPrice price = fromStocksCache.GetPrice(date);
      line.Price = price.Price * num;
      line.CurrencyId = price.CurrencyId;
    }
  }

  protected override async Task OnSelectedLineEditAsync()
  {
    StockSlipDetailsViewModel detailsViewModel = this;
    Stock stocksCacheAsync = await detailsViewModel.GetFromStocksCacheAsync(detailsViewModel.SelectedLine.StockId);
    IMvxNavigationService navigationService = detailsViewModel.NavigationService;
    StockSlipDetailsLineEditViewModel.Params @params = new StockSlipDetailsLineEditViewModel.Params();
    @params.StockCode = stocksCacheAsync.Code;
    @params.StockName = stocksCacheAsync.Name;
    @params.Quantity = detailsViewModel.SelectedLine.Quantity;
    @params.UnitId = detailsViewModel.SelectedLine.UnitId;
    @params.Units = (IEnumerable<StockUnit>) stocksCacheAsync.Units;
    @params.Price = detailsViewModel.SelectedLine.Price;
    @params.CurrencyId = detailsViewModel.SelectedLine.CurrencyId;
    @params.Currencies = detailsViewModel.Currencies.List;
    @params.IsPriceEditable = detailsViewModel.Details.IsPriceEditable;
    CancellationToken cancellationToken = new CancellationToken();
    StockTransactionDetailsLineEditViewModel.Result result = await navigationService.Navigate<StockSlipDetailsLineEditViewModel, StockSlipDetailsLineEditViewModel.Params, StockTransactionDetailsLineEditViewModel.Result>(@params, cancellationToken: cancellationToken);
    if (result == null)
      return;
    detailsViewModel.SelectedLine.Quantity = result.Quantity;
    detailsViewModel.SelectedLine.UnitId = result.UnitId;
    if (!detailsViewModel.Details.IsPriceEditable)
      return;
    detailsViewModel.SelectedLine.Price = result.Price;
    detailsViewModel.SelectedLine.CurrencyId = result.CurrencyId;
  }

  public ICommand PrintCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnPrintCommandAsync), (Func<bool>) (() => !this.IsBusy && !this.IsDirty));
    }
  }

  protected virtual async Task OnPrintCommandAsync()
  {
    StockSlipDetailsViewModel detailsViewModel = this;
    await detailsViewModel._printingService.PrintStockSlip(detailsViewModel.Details, true);
  }
}
