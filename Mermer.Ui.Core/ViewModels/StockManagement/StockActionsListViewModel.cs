// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockActionsListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Commerce.Models;
using Mermer.Common.Settings;
using Mermer.Enterprise.Models;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Warehousing.Models;
using Mermer.Data.Storage;
using Mermer.Mvvm.Messages;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockActionsListViewModel : 
  ListViewModelBaseWithFilterDate<StockActionWithData>,
  IMvxViewModel<StockActionsFilter>,
  IMvxViewModel
{
  private readonly IConfigurator _configurator;
  private readonly IStockActionsRepository _repository;
  private readonly IRepository<Stock> _stocksRepository;
  private readonly MvxSubscriptionToken _messageToken;
    private List<object> _selectedWarehouseIds = new List<object>();
    private string _stockId;
  private string _selectedStockMessage;
  private StockActionsFilter _parameter;
  private bool _loaded;

  public StockActionsListViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    StockSearcher stockSearcher,
    Reference<Warehouse> warehouses,
    IStockActionsRepository repository,
    IRepository<Stock> stocksRepository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._repository = repository;
    this._stocksRepository = stocksRepository;
    this._messageToken = messenger.Subscribe<DocumentModified<StockAction>>((Action<DocumentModified<StockAction>>) (async m => await this.Initialize()), MvxReference.Strong);
    this.Warehouses = warehouses;
    this.StockSearcher = stockSearcher;
    this.StockSearcher.ResultSelected += new SearchResultSelected(this.StockSearcher_ResultSelected);
    this.Types = new LocalizedTransactionTypes("Repricing");
  }

  public System.Collections.Generic.List<object> SelectedWarehouseIds
  {
    get => this._selectedWarehouseIds;
    set
    {
      if (this._selectedWarehouseIds != null && value != null && this._selectedWarehouseIds.SequenceEqual<object>((IEnumerable<object>) value) || !this.SetProperty<System.Collections.Generic.List<object>>(ref this._selectedWarehouseIds, value, nameof (SelectedWarehouseIds)) || this.IsBusy)
        return;
      this.Initialize();
    }
  }

  public string[] WarehouseIds
  {
    get
    {
      System.Collections.Generic.List<object> selectedWarehouseIds = this.SelectedWarehouseIds;
      return (selectedWarehouseIds != null ? selectedWarehouseIds.Cast<string>().ToArray<string>() : (string[]) null) ?? Array.Empty<string>();
    }
  }

  public virtual string StockId
  {
    get => this._stockId;
    set
    {
      if (this.SetProperty<string>(ref this._stockId, value, nameof (StockId)) && !this.IsBusy)
        this.Initialize();
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.StockIdSelected));
    }
  }

  public bool StockIdSelected => !string.IsNullOrEmpty(this.StockId);

  public virtual string SelectedStockMessage
  {
    get => this._selectedStockMessage;
    set
    {
      this.SetProperty<string>(ref this._selectedStockMessage, value, nameof (SelectedStockMessage));
    }
  }

  public StockSearcher StockSearcher { get; }

  public Reference<Warehouse> Warehouses { get; }

  public LocalizedTransactionTypes Types { get; }

  private void StockSearcher_ResultSelected(StockSearcher searcher, StockSearchResult result)
  {
    this.SelectedStockMessage = this["Showing actions for stock: {0} | {1}", new object[2]
    {
      (object) result.Code,
      (object) result.Name
    }];
    this.StockId = result.Id;
  }

  public void Prepare(StockActionsFilter parameter) => this._parameter = parameter;

    protected override async Task PreLoad()
    {
        if (!_loaded)
        {
            if (_parameter != null)
            {
                SelectedWarehouseIds = _parameter.WarehouseIds.Cast<object>().ToList();
                StockId = _parameter.StockId;
                DateFilterFrom = _parameter.DateFrom;
                DateFilterTill = _parameter.DateTill;

                if (!string.IsNullOrEmpty(StockId))
                {
                    Stock async = await _stocksRepository.GetAsync(StockId);
                    SelectedStockMessage = this["Showing actions for stock: {0} | {1}", async.Code, async.Name];
                }
            }
            else
            {
                AppSettings configAsync = await _configurator.GetConfigAsync<AppSettings>();

                // ВИПРАВЛЕНО: Захищаємо список від null-елемента
                if (configAsync != null && !string.IsNullOrEmpty(configAsync.DefaultWarehouseId))
                {
                    SelectedWarehouseIds = new List<object> { configAsync.DefaultWarehouseId };
                }
                else
                {
                    SelectedWarehouseIds = new List<object>(); // Чистий порожній список
                }
            }
        }

        _loaded = true;

        if (string.IsNullOrEmpty(StockId))
            SelectedStockMessage = this["Showing actions for all stocks"];

        await Task.WhenAll(
            base.PreLoad(),
            Warehouses.Initialize(),
            StockSearcher.Initialize()
        );
    }

    protected override Task OnLoad()
  {
    if (this._parameter == null)
      return base.OnLoad();
    this._parameter = (StockActionsFilter) null;
    return this.LoadByDateAsync(false);
  }

  protected override Task<int> CountFilteredListByDateAsync(DateTime from, DateTime till)
  {
    return this._repository.CountAsync(new DateTime?(from), new DateTime?(till), this.StockId, this.WarehouseIds);
  }

  protected override Task<int> CountFilteredListAsync(ListFilter filter)
  {
    return this._repository.CountAsync(new DateTime?(), new DateTime?(), this.StockId, this.WarehouseIds);
  }

  protected override Task<IEnumerable<StockActionWithData>> GetFilteredListByDateAsync(
    DateTime from,
    DateTime till)
  {
    return this._repository.GetAsync(new DateTime?(from), new DateTime?(till), this.StockId, this.WarehouseIds);
  }

  protected override Task<IEnumerable<StockActionWithData>> GetFilteredListAsync(ListFilter filter)
  {
    return this._repository.GetAsync(new DateTime?(), new DateTime?(), this.StockId, this.WarehouseIds);
  }

    protected override Task<int> CountListAsync(params Expression<Func<StockActionWithData, bool>>[] predicates)
    {
        return Task.FromResult(0);
    }

    protected override Task<IEnumerable<StockActionWithData>> GetListAsync(params Expression<Func<StockActionWithData, bool>>[] predicates)
    {
        return this._repository.GetAsync(null, null, this.StockId, this.WarehouseIds);
    }

    protected override Expression<Func<StockActionWithData, bool>> GetDateFilter(DateTime from, DateTime till)
    {
        return x => true;
    }

    public ICommand RemoveSelectedStockId
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnRemoveSelectedStockId), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private void OnRemoveSelectedStockId() => this.StockId = (string) null;

  public ICommand SelectOrViewDetailsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectOrViewDetailsAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedItem != null));
    }
  }

  private Task OnSelectOrViewDetailsAsync()
  {
    switch (this.SelectedItem.TransactionType)
    {
      case "Purchase":
      case "PurchaseReturn":
      case "Sales":
      case "SalesReturn":
        return this.NavigationService.Navigate<DetailsViewModel<Invoice>, string>(this.SelectedItem.TransactionId);
      case "RevisionDeficit":
      case "RevisionExceed":
      case "StockOpening":
      case "StockSpoilage":
      case "StockUsage":
        return this.NavigationService.Navigate<DetailsViewModel<StockSlip>, string>(this.SelectedItem.TransactionId);
      case "StockTransferDestination":
      case "StockTransferSource":
        return this.NavigationService.Navigate<DetailsViewModel<StockTransfer>, string>(this.SelectedItem.TransactionId);
      default:
        return Task.CompletedTask;
    }
  }

  public override void Dispose()
  {
    base.Dispose();
    this._messageToken?.Dispose();
  }
}
