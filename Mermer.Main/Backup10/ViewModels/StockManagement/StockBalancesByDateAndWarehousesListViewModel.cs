// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockBalancesByDateAndWarehousesListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Data.Extenders;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockBalancesByDateAndWarehousesListViewModel : BaseViewModel
{
  private readonly IStockBalancesRepository _balancesRepository;
  private DateTime _date = DateTime.Today;
  private System.Collections.Generic.List<object> _selectedWarehouseIds;
  private string _displayCurrencyId;
  private ColumnDescription[] _columns;
  private ObservableCollection<string> _selectedStockIds;
  private bool _showAllStocks;
  private ObservableCollection<StockBalanceByWarehouses> _list;
  private StockBalanceByWarehouses _selectedItem;
  private bool _loaded;

  public StockBalancesByDateAndWarehousesListViewModel(
    StockSearcher stockSearcher,
    Reference<Currency> currencies,
    Reference<Warehouse> warehouses,
    IMvxNavigationService navigationService,
    IStockBalancesRepository balancesRepository,
    IUserInteractionService userInteractionService)
    : base(navigationService, userInteractionService)
  {
    this._balancesRepository = balancesRepository;
    this.Currencies = currencies;
    this.Warehouses = warehouses;
    this.StockSearcher = stockSearcher;
    this.StockSearcher.ResultSelected += new SearchResultSelected(this.OnStockSearcherOnResultSelected);
    this.SelectedStockIds = new ObservableCollection<string>();
  }

  public StockSearcher StockSearcher { get; }

  public Reference<Currency> Currencies { get; }

  public Reference<Warehouse> Warehouses { get; }

  public virtual DateTime Date
  {
    get => this._date;
    set
    {
      if (!this.SetProperty<DateTime>(ref this._date, value, nameof (Date)) || this.IsBusy)
        return;
      this.Initialize();
    }
  }

  public DateTime DateFilterInclusive
  {
    get
    {
      DateTime dateTime = this.Date;
      dateTime = dateTime.AddDays(1.0);
      return dateTime.Date;
    }
  }

  public System.Collections.Generic.List<object> SelectedWarehouseIds
  {
    get => this._selectedWarehouseIds;
    set
    {
      if (this._selectedWarehouseIds != null && value != null && this._selectedWarehouseIds.SequenceEqual<object>((IEnumerable<object>) value))
        return;
      this.SetProperty<System.Collections.Generic.List<object>>(ref this._selectedWarehouseIds, value, nameof (SelectedWarehouseIds));
      this.GenerateColumns();
      if (this.IsBusy)
        return;
      this.Initialize();
    }
  }

  public IEnumerable<string> WarehouseIds
  {
    get
    {
      System.Collections.Generic.List<object> selectedWarehouseIds = this.SelectedWarehouseIds;
      return (selectedWarehouseIds != null ? selectedWarehouseIds.Cast<string>() : (IEnumerable<string>) null) ?? (IEnumerable<string>) Array.Empty<string>();
    }
  }

  public virtual string DisplayCurrencyId
  {
    get => this._displayCurrencyId;
    set
    {
      if (this.SetProperty<string>(ref this._displayCurrencyId, value, nameof (DisplayCurrencyId)) && !this.IsBusy)
        this.Initialize();
      this.StockSearcher.CurrencyId = this._displayCurrencyId;
    }
  }

  public ColumnDescription[] Columns
  {
    get => this._columns;
    set => this.SetProperty<ColumnDescription[]>(ref this._columns, value, nameof (Columns));
  }

  public ObservableCollection<string> SelectedStockIds
  {
    get => this._selectedStockIds;
    set
    {
      if (this._selectedStockIds != null)
        this._selectedStockIds.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.SelectedStockIds_CollectionChanged);
      this.SetProperty<ObservableCollection<string>>(ref this._selectedStockIds, value, nameof (SelectedStockIds));
      if (this._selectedStockIds == null)
        return;
      this._selectedStockIds.CollectionChanged += new NotifyCollectionChangedEventHandler(this.SelectedStockIds_CollectionChanged);
    }
  }

  public bool ShowAllStocks
  {
    get => this._showAllStocks;
    set => this.SetProperty<bool>(ref this._showAllStocks, value, nameof (ShowAllStocks));
  }

  public virtual ObservableCollection<StockBalanceByWarehouses> List
  {
    get => this._list;
    set
    {
      this.SetProperty<ObservableCollection<StockBalanceByWarehouses>>(ref this._list, value, nameof (List));
    }
  }

  public StockBalanceByWarehouses SelectedItem
  {
    get => this._selectedItem;
    set
    {
      if (!this.SetProperty<StockBalanceByWarehouses>(ref this._selectedItem, value, nameof (SelectedItem)))
        return;
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsItemSelected));
    }
  }

  public bool IsItemSelected => this.SelectedItem != null;

  protected override async Task PreLoad()
  {
    await Task.WhenAll(base.PreLoad(), this.Currencies.Initialize(), this.Warehouses.Initialize(), this.StockSearcher.Initialize());
    if (!this._loaded && string.IsNullOrEmpty(this.DisplayCurrencyId))
      this.DisplayCurrencyId = this.Currencies.List.Single<Currency>((Func<Currency, bool>) (x => x.IsDefault)).Id;
    this._loaded = true;
  }

  protected override async Task OnLoad()
  {
    this.List = new ObservableCollection<StockBalanceByWarehouses>(await this._balancesRepository.GetByDateAndWarehousesAsync(this.DateFilterInclusive, this.WarehouseIds, this.DisplayCurrencyId, this.ShowAllStocks ? (IEnumerable<string>) null : (IEnumerable<string>) this.SelectedStockIds));
  }

  private async Task AddLineAsync(string stockId)
  {
    StockBalancesByDateAndWarehousesListViewModel warehousesListViewModel = this;
    warehousesListViewModel.IsBusy = true;
    try
    {
      IEnumerable<StockBalanceByWarehouses> andWarehousesAsync = await warehousesListViewModel._balancesRepository.GetByDateAndWarehousesAsync(warehousesListViewModel.DateFilterInclusive, warehousesListViewModel.WarehouseIds, warehousesListViewModel.DisplayCurrencyId, (IEnumerable<string>) new string[1]
      {
        stockId
      });
      warehousesListViewModel.List.Add(andWarehousesAsync.Single<StockBalanceByWarehouses>());
    }
    catch (Exception ex)
    {
      warehousesListViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    warehousesListViewModel.IsBusy = false;
  }

  public ICommand AddAllStocksCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnAddAllStocksCommandAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnAddAllStocksCommandAsync()
  {
    StockBalancesByDateAndWarehousesListViewModel warehousesListViewModel = this;
    warehousesListViewModel.IsBusy = true;
    try
    {
      warehousesListViewModel.SelectedStockIds.Clear();
      warehousesListViewModel.ShowAllStocks = true;
      IEnumerable<StockBalanceByWarehouses> andWarehousesAsync = await warehousesListViewModel._balancesRepository.GetByDateAndWarehousesAsync(warehousesListViewModel.DateFilterInclusive, warehousesListViewModel.WarehouseIds, warehousesListViewModel.DisplayCurrencyId);
      warehousesListViewModel.List = new ObservableCollection<StockBalanceByWarehouses>(andWarehousesAsync);
      warehousesListViewModel.SelectedStockIds = new ObservableCollection<string>(warehousesListViewModel.List.Select<StockBalanceByWarehouses, string>((Func<StockBalanceByWarehouses, string>) (x => x.StockId)));
      warehousesListViewModel.SubCaption = warehousesListViewModel["All Records", Array.Empty<object>()];
    }
    catch (Exception ex)
    {
      warehousesListViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    warehousesListViewModel.IsBusy = false;
  }

  public ICommand SelectedLineDeleteCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnSelectedLineDeleteCommand), (Func<bool>) (() => !this.IsBusy && this.SelectedItem != null));
    }
  }

  private void OnSelectedLineDeleteCommand()
  {
    if (this.SelectedStockIds.Contains(this.SelectedItem.StockId))
      this.SelectedStockIds.Remove(this.SelectedItem.StockId);
    this.SelectedItem = this.List.RemoveWithSelection<StockBalanceByWarehouses>(this.SelectedItem);
  }

  public ICommand ReloadCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnReloadAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private Task OnReloadAsync() => this.Initialize();

  private void OnStockSearcherOnResultSelected(StockSearcher searcher, StockSearchResult result)
  {
    if (this.SelectedStockIds.Contains(result.Id))
      return;
    this.SelectedStockIds.Add(result.Id);
  }

  private async void SelectedStockIds_CollectionChanged(
    object sender,
    NotifyCollectionChangedEventArgs e)
  {
    StockBalancesByDateAndWarehousesListViewModel warehousesListViewModel = this;
    if (!warehousesListViewModel.IsBusy && e.NewItems != null)
    {
      warehousesListViewModel.ShowAllStocks = false;
      foreach (string stockId in e.NewItems.Cast<string>())
        await warehousesListViewModel.AddLineAsync(stockId);
    }
    warehousesListViewModel.SubCaption = warehousesListViewModel["By Selected Stocks", Array.Empty<object>()];
  }

  private void GenerateColumns()
  {
    this.Columns = this.Warehouses.List.Where<Warehouse>((Func<Warehouse, bool>) (x => this.WarehouseIds.Contains<string>(x.Id))).Select<Warehouse, ColumnDescription>((Func<Warehouse, ColumnDescription>) (x => new ColumnDescription(x.Id, x.Name))).ToArray<ColumnDescription>();
  }
}
