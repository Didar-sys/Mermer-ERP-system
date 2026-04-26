// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.StockTransferDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.Services;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Binyat.Warehousing.Models;
using Payhas.Data;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing;

public class StockTransferDetailsViewModel : 
  StockTransactionDetailsViewModel<StockTransfer, StockTransferLine>,
  IMvxViewModel<StockTransferDetailsViewModel.Params>,
  IMvxViewModel
{
  private readonly IPrintingService _printingService;
  private string _sourceWarehouseId;
  private string _destinationWarehouseId;

  public StockTransferDetailsViewModel(
    CopyCreate copyCreate,
    IConfigurator configurator,
    ILoginService loginService,
    StockSearcher stockSearcher,
    Reference<Currency> currencies,
    Reference<Warehouse> warehouses,
    IPrintingService printingService,
    IStocksRepository stocksRepository,
    IRepository<StockTransfer> repository,
    IListAuthorizer<StockTransfer> authorizer,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IUserInteractionService userInteractionService)
    : base(copyCreate, repository, authorizer, configurator, loginService, stockSearcher, currencies, warehouses, stocksRepository, navigationService, codegentor, userInteractionService)
  {
    this._printingService = printingService;
  }

  public void Prepare(StockTransferDetailsViewModel.Params parameter)
  {
    this._sourceWarehouseId = parameter.SourceWarehouseId;
    this._destinationWarehouseId = parameter.DestinationWarehouseId;
    this.Prepare(parameter.Lines);
  }

  protected override async Task OnLoad()
  {
    StockTransferDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (!string.IsNullOrEmpty(detailsViewModel.ItemId))
      return;
    if (!string.IsNullOrEmpty(detailsViewModel._sourceWarehouseId))
      detailsViewModel.Details.WarehouseId = detailsViewModel._sourceWarehouseId;
    if (string.IsNullOrEmpty(detailsViewModel._destinationWarehouseId))
      return;
    detailsViewModel.Details.DestinationWarehouseId = detailsViewModel._destinationWarehouseId;
  }

  protected override async Task PostLoad()
  {
    StockTransferDetailsViewModel detailsViewModel = this;
    IEnumerable<string> usedWarehouseIds = ((IEnumerable<string>) new string[2]
    {
      detailsViewModel.Details.WarehouseId,
      detailsViewModel.Details.DestinationWarehouseId
    }).Distinct<string>();
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__1();
    detailsViewModel.Warehouses.Filter = (Func<Warehouse, bool>) (x => !x.IsDisabled || usedWarehouseIds.Contains<string>(x.Id));
  }

  protected override async Task<bool> OnSaveAsync()
  {
    StockTransferDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    if (!await detailsViewModel.\u003C\u003En__2())
      return false;
    await detailsViewModel._printingService.PrintStockTransfer(detailsViewModel.Details);
    return true;
  }

  protected override StockTransferLine CreateNewLine(
    Stock stock,
    Decimal? quantity = null,
    string unitId = null,
    Decimal? price = null,
    string currencyId = null)
  {
    StockTransferLine newLine = base.CreateNewLine(stock, quantity, unitId, price, currencyId);
    newLine.ReceivedId = Guid.NewGuid().ToString();
    newLine.ReceivedUnitId = newLine.UnitId;
    return newLine;
  }

  protected override async Task OnSelectedLineEditAsync()
  {
    StockTransferDetailsViewModel detailsViewModel = this;
    Stock stocksCacheAsync = await detailsViewModel.GetFromStocksCacheAsync(detailsViewModel.SelectedLine.StockId);
    IMvxNavigationService navigationService = detailsViewModel.NavigationService;
    StockTransferDetailsLineEditViewModel.Params @params = new StockTransferDetailsLineEditViewModel.Params();
    @params.StockCode = stocksCacheAsync.Code;
    @params.StockName = stocksCacheAsync.Name;
    @params.Quantity = detailsViewModel.SelectedLine.Quantity;
    @params.UnitId = detailsViewModel.SelectedLine.UnitId;
    @params.ReceivedQuantity = detailsViewModel.SelectedLine.ReceivedQuantity;
    @params.ReceivedUnitId = detailsViewModel.SelectedLine.ReceivedUnitId;
    @params.Units = (IEnumerable<StockUnit>) stocksCacheAsync.Units;
    CancellationToken cancellationToken = new CancellationToken();
    StockTransferDetailsLineEditViewModel.Result result = await navigationService.Navigate<StockTransferDetailsLineEditViewModel, StockTransferDetailsLineEditViewModel.Params, StockTransferDetailsLineEditViewModel.Result>(@params, cancellationToken: cancellationToken);
    if (result == null)
      return;
    detailsViewModel.SelectedLine.Quantity = result.Quantity;
    detailsViewModel.SelectedLine.UnitId = result.UnitId;
    detailsViewModel.SelectedLine.ReceivedQuantity = result.ReceivedQuantity;
    detailsViewModel.SelectedLine.ReceivedUnitId = result.ReceivedUnitId;
  }

  protected override bool AllowStockTracking()
  {
    return this.Details != null && this.Details.IsCompleted && !this.Details.IsConflicted;
  }

  public ICommand SelectDestinationWarehouseCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.SelectDestinationWarehouseAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task SelectDestinationWarehouseAsync()
  {
    StockTransferDetailsViewModel detailsViewModel = this;
    StockTransfer stockTransfer = detailsViewModel.Details;
    stockTransfer.DestinationWarehouseId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Warehouse>, string, string>(detailsViewModel.Details.DestinationWarehouseId ?? Guid.Empty.ToString());
    stockTransfer = (StockTransfer) null;
  }

  public ICommand EqualizeSentAndRecieved
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnEqualizeSentAndRecieved), (Func<bool>) (() =>
      {
        if (this.IsBusy || !this.HasSaveAccess)
          return false;
        StockTransfer details = this.Details;
        bool? nullable;
        if (details == null)
        {
          nullable = new bool?();
        }
        else
        {
          WatchedObservableCollection<StockTransferLine> lines = details.Lines;
          nullable = lines != null ? new bool?(lines.Any<StockTransferLine>()) : new bool?();
        }
        return nullable.GetValueOrDefault();
      }));
    }
  }

  private void OnEqualizeSentAndRecieved()
  {
    foreach (StockTransferLine line in (Collection<StockTransferLine>) this.Details.Lines)
    {
      line.ReceivedQuantity = line.Quantity;
      line.ReceivedUnitId = line.UnitId;
    }
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
    StockTransferDetailsViewModel detailsViewModel = this;
    await detailsViewModel._printingService.PrintStockTransfer(detailsViewModel.Details, true);
  }

  public class Params
  {
    public string SourceWarehouseId { get; set; }

    public string DestinationWarehouseId { get; set; }

    public IEnumerable<CopyCreateLine> Lines { get; set; }
  }
}
