// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Transactions.StockTransactionDetailsViewModel`2
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Authorization.Services;
using Mermer.Common.Settings;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Transactions.Models;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Ui.Core.ViewModels.StockManagement;
using Mermer.Data;
using Mermer.Data.Authorizers;
using Mermer.Data.Extenders;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Transactions;

public class StockTransactionDetailsViewModel<T, TLine> : 
  TransactionDetailsViewModel<T, TLine>,
  IMvxViewModel<IEnumerable<CopyCreateLine>>,
  IMvxViewModel
  where T : StockTransaction<TLine>
  where TLine : StockTransactionLine
{
  private readonly IStocksRepository _stocksRepository;
  private Decimal _addQuantity = 1M;
  private ObservableCollection<Stock> _stocksCache;
  private bool _allowReporting;
  private IEnumerable<CopyCreateLine> _stockLineCopies;

  public StockTransactionDetailsViewModel(
    CopyCreate copyCreate,
    IRepository<T> repository,
    IListAuthorizer<T> authorizer,
    IConfigurator configurator,
    ILoginService loginService,
    StockSearcher stockSearcher,
    Reference<Currency> currencies,
    Reference<Warehouse> warehouses,
    IStocksRepository stocksRepository,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IUserInteractionService userInteractionService)
    : base(configurator, repository, authorizer, loginService, currencies, navigationService, codegentor, userInteractionService)
  {
    this._stocksRepository = stocksRepository;
    this.Warehouses = warehouses;
    this.CopyCreate = copyCreate;
    this.CopyCreate.GetLines = (Func<IEnumerable<CopyCreateLine>>) (() => this.Details.Lines.Select<TLine, CopyCreateLine>((Func<TLine, CopyCreateLine>) (x => new CopyCreateLine()
    {
      StockId = x.StockId,
      Quantity = new Decimal?(x.Quantity),
      UnitId = x.UnitId,
      Price = new Decimal?(x.Price),
      CurrencyId = x.CurrencyId
    })));
    this.StockSearcher = stockSearcher;
    this.StockSearcher.ResultSelected += new SearchResultSelected(this.StockSearcher_ResultSelected);
  }

  public CopyCreate CopyCreate { get; }

  public StockSearcher StockSearcher { get; }

  public Reference<Warehouse> Warehouses { get; }

  public virtual Decimal AddQuantity
  {
    get => this._addQuantity;
    set => this.SetProperty<Decimal>(ref this._addQuantity, value, nameof (AddQuantity));
  }

  public ObservableCollection<Stock> StocksCache
  {
    get => this._stocksCache;
    set
    {
      this.SetProperty<ObservableCollection<Stock>>(ref this._stocksCache, value, nameof (StocksCache));
    }
  }

  public virtual bool AllowReporting
  {
    get => this._allowReporting;
    set => this.SetProperty<bool>(ref this._allowReporting, value, nameof (AllowReporting));
  }

  public void Prepare(IEnumerable<CopyCreateLine> parameter) => this._stockLineCopies = parameter;

  protected override Task PreLoad()
  {
    this.StocksCache = new ObservableCollection<Stock>();
    return Task.WhenAll(base.PreLoad(), this.Warehouses.Initialize(), this.StockSearcher.Initialize());
  }

  protected override async Task OnLoad()
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    ConnectionSettings configAsync = await detailsViewModel.Configurator.GetConfigAsync<ConnectionSettings>();
    detailsViewModel.AllowReporting = configAsync.AllowReporting;
    if (!string.IsNullOrEmpty(detailsViewModel.ItemId) || !string.IsNullOrEmpty(detailsViewModel.Details.WarehouseId))
      return;
    detailsViewModel.Details.WarehouseId = detailsViewModel.AppSettings.DefaultWarehouseId;
  }

  protected override async Task PostLoad()
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__1();
    detailsViewModel.Details.StockUnitConverterRequested += new Mermer.Transactions.Models.StockUnitConverter(detailsViewModel.StockUnitConverter);
    if (detailsViewModel.Details.StockUnitConvertions == null)
      detailsViewModel.Details.StockUnitConvertions = new WatchedObservableCollection<StockUnitConvertion>();
    if (detailsViewModel.Details.Overheads == null)
      detailsViewModel.Details.Overheads = new WatchedObservableCollection<StockTransactionOverhead>();
    if (string.IsNullOrEmpty(detailsViewModel.ItemId) && detailsViewModel._stockLineCopies != null)
    {
      await detailsViewModel.UpdateStocksCacheAsync(detailsViewModel._stockLineCopies.Select<CopyCreateLine, string>((Func<CopyCreateLine, string>) (x => x.StockId)).ToArray<string>());
      foreach (CopyCreateLine stockLineCopy in detailsViewModel._stockLineCopies)
      {
        TLine newLineAsync = await detailsViewModel.CreateNewLineAsync(stockLineCopy.StockId, stockLineCopy.Quantity, stockLineCopy.UnitId, stockLineCopy.Price, stockLineCopy.CurrencyId);
        detailsViewModel.Details.Lines.Add(newLineAsync);
      }
    }
    detailsViewModel.StockSearcher.WarehouseId = detailsViewModel.Details.WarehouseId;
    detailsViewModel.StockSearcher.CurrencyId = detailsViewModel.Details.DisplayCurrencyId;
    detailsViewModel.StockSearcher.ShowLastPurchasePrice = detailsViewModel.AppSettings.ShowLastPurchasePriceOnSearch;
    await detailsViewModel.LoadStocksCache();
    detailsViewModel.Details.RaisePropertyChanged("LineQuantitiesSum");
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Warehouses.Filter = new Func<Warehouse, bool>(detailsViewModel.\u003CPostLoad\u003Eb__27_1);
  }

  public override async Task Initialize()
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__2();
    if (detailsViewModel._stockLineCopies == null)
      return;
    detailsViewModel._stockLineCopies = (IEnumerable<CopyCreateLine>) null;
    detailsViewModel.IsDirty = true;
  }

  protected override void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "WarehouseId")
      this.StockSearcher.WarehouseId = this.Details.WarehouseId;
    else if (e.PropertyName == "DisplayCurrencyId")
      this.StockSearcher.CurrencyId = this.Details.DisplayCurrencyId;
    base.Details_PropertyChanged(sender, e);
  }

  private StockUnitConvertion StockUnitConverter(string stockId, string unitId)
  {
    Stock fromStocksCache = this.GetFromStocksCache(stockId);
    StockUnit stockUnit1;
    if (fromStocksCache == null)
    {
      stockUnit1 = (StockUnit) null;
    }
    else
    {
      ObservableCollection<StockUnit> units = fromStocksCache.Units;
      stockUnit1 = units != null ? units.SingleOrDefault<StockUnit>((Func<StockUnit, bool>) (x => x.Id == unitId)) : (StockUnit) null;
    }
    StockUnit stockUnit2 = stockUnit1;
    if (stockUnit2 == null)
      return (StockUnitConvertion) null;
    return new StockUnitConvertion()
    {
      StockId = fromStocksCache.Id,
      UnitId = stockUnit2.Id,
      Multiplier = stockUnit2.Multiplier,
      Divider = stockUnit2.Divider
    };
  }

  private async Task LoadStocksCache()
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    await detailsViewModel.UpdateStocksCacheAsync(detailsViewModel.Details.Lines.Select<TLine, string>((Func<TLine, string>) (x => x.StockId)).ToArray<string>());
    // ISSUE: explicit non-virtual call
    __nonvirtual (detailsViewModel.RaisePropertyChanged<ObservableCollection<Stock>>((Expression<Func<ObservableCollection<Stock>>>) (() => detailsViewModel.StocksCache)));
  }

  protected Stock GetFromStocksCache(string stockId)
  {
    return this.GetFromStocksCacheAsync(stockId).GetAwaiter().GetResult();
  }

  protected async Task<Stock> GetFromStocksCacheAsync(string stockId)
  {
    Stock stocksCacheAsync = this.StocksCache.SingleOrDefault<Stock>((Func<Stock, bool>) (x => x.Id == stockId));
    if (stocksCacheAsync == null)
    {
      stocksCacheAsync = await this._stocksRepository.GetAsync(stockId);
      this.StocksCache.Add(stocksCacheAsync);
    }
    return stocksCacheAsync;
  }

  protected async Task UpdateStocksCacheAsync(params string[] stockIds)
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    string[] array = ((IEnumerable<string>) stockIds).Distinct<string>().Where<string>(new Func<string, bool>(detailsViewModel.\u003CUpdateStocksCacheAsync\u003Eb__34_0)).ToArray<string>();
    if (!((IEnumerable<string>) array).Any<string>())
      return;
    foreach (Stock stock in await detailsViewModel._stocksRepository.GetListAsync(array))
      detailsViewModel.StocksCache.Add(stock);
  }

  protected async Task UpdateStocksCacheByCodeAsync(params string[] stockCodes)
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    string[] stockCodesToAdd = ((IEnumerable<string>) stockCodes).Distinct<string>().Where<string>(new Func<string, bool>(detailsViewModel.\u003CUpdateStocksCacheByCodeAsync\u003Eb__35_0)).ToArray<string>();
    if (!((IEnumerable<string>) stockCodesToAdd).Any<string>())
    {
      stockCodesToAdd = (string[]) null;
    }
    else
    {
      for (int i = 0; i < stockCodesToAdd.Length; i += 100)
      {
        string[] stockCodesToAddPartial = ((IEnumerable<string>) stockCodesToAdd).Skip<string>(i).Take<string>(100).ToArray<string>();
        IStocksRepository stocksRepository = detailsViewModel._stocksRepository;
        Expression<Func<Stock, bool>>[] expressionArray = new Expression<Func<Stock, bool>>[1]
        {
          (Expression<Func<Stock, bool>>) (x => stockCodesToAddPartial.Contains<string>(x.Code))
        };
        foreach (Stock stock in await stocksRepository.GetAsync(expressionArray))
          detailsViewModel.StocksCache.Add(stock);
      }
      stockCodesToAdd = (string[]) null;
    }
  }

  protected async Task<Stock> GetFromStocksCacheByCodeAsync(string stockCode)
  {
    Stock cacheByCodeAsync = this.StocksCache.SingleOrDefault<Stock>((Func<Stock, bool>) (x => x.Code == stockCode));
    if (cacheByCodeAsync == null)
    {
      cacheByCodeAsync = (await this._stocksRepository.GetAsync((Expression<Func<Stock, bool>>) (x => x.Code == stockCode))).Single<Stock>();
      this.StocksCache.Add(cacheByCodeAsync);
    }
    return cacheByCodeAsync;
  }

  protected virtual async void StockSearcher_ResultSelected(
    StockSearcher searcher,
    StockSearchResult result)
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    TLine newLineAsync = await detailsViewModel.CreateNewLineAsync(result.Id, new Decimal?(detailsViewModel.AddQuantity), result.UnitId, new Decimal?(result.Price), result.CurrencyId);
    detailsViewModel.Details.Lines.Add(newLineAsync);
    detailsViewModel.SelectedLine = newLineAsync;
    detailsViewModel.AddQuantity = 1M;
    if (!detailsViewModel.AppSettings.OpenEditorWhenAdding)
      return;
    detailsViewModel.SelectedLineEditCommand.Execute((object) null);
  }

  protected virtual async Task<TLine> CreateNewLineAsync(
    string stockId,
    Decimal? quantity = null,
    string unitId = null,
    Decimal? price = null,
    string currencyId = null)
  {
    return this.CreateNewLine(await this.GetFromStocksCacheAsync(stockId), quantity, unitId, price, currencyId);
  }

  protected virtual TLine CreateNewLine(
    Stock stock,
    Decimal? quantity = null,
    string unitId = null,
    Decimal? price = null,
    string currencyId = null)
  {
    if (price.HasValue && price.Value == 0M)
      price = new Decimal?();
    TLine instance = Activator.CreateInstance<TLine>();
    instance.Id = Guid.NewGuid().ToString();
    instance.StockId = stock.Id;
    instance.Quantity = quantity.GetValueOrDefault();
    instance.UnitId = unitId ?? stock.UnitId;
    if (!price.HasValue || currencyId == null)
    {
      CurrencyConvertion currencyConvertion = this.Details.CurrencyConverter(stock.CurrencyId);
      price = new Decimal?(this.Details.GetDisplayAmount(stock.Price * currencyConvertion.Multiplier / currencyConvertion.Divider));
      currencyId = this.Details.DisplayCurrencyId;
    }
    instance.Price = Math.Round(price.Value, this.Currencies.List.Single<Currency>((Func<Currency, bool>) (x => x.Id == currencyId)).Decimals);
    instance.CurrencyId = currencyId;
    return instance;
  }

  public ICommand SelectedLineMinusOneCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnSelectedLineMinusOne), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

  private void OnSelectedLineMinusOne()
  {
    this.SelectedLine.Quantity -= 1M;
    if (!(this.SelectedLine.Quantity == 0M))
      return;
    this.SelectedLineDeleteCommand.Execute((object) null);
  }

  public ICommand SelectedLinePlusOneCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnSelectedLinePlusOne), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

  private void OnSelectedLinePlusOne() => this.SelectedLine.Quantity += 1M;

  public ICommand SelectedLineEditCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectedLineEditAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

  protected virtual async Task OnSelectedLineEditAsync()
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    Stock stocksCacheAsync = await detailsViewModel.GetFromStocksCacheAsync(detailsViewModel.SelectedLine.StockId);
    IMvxNavigationService navigationService = detailsViewModel.NavigationService;
    StockTransactionDetailsLineEditViewModel.Params @params = new StockTransactionDetailsLineEditViewModel.Params();
    @params.StockCode = stocksCacheAsync.Code;
    @params.StockName = stocksCacheAsync.Name;
    @params.Quantity = detailsViewModel.SelectedLine.Quantity;
    @params.UnitId = detailsViewModel.SelectedLine.UnitId;
    @params.Units = (IEnumerable<StockUnit>) stocksCacheAsync.Units;
    @params.Price = detailsViewModel.SelectedLine.Price;
    @params.CurrencyId = detailsViewModel.SelectedLine.CurrencyId;
    @params.Currencies = detailsViewModel.Currencies.List;
    @params.ActionDate = new DateTime?(detailsViewModel.Details.Date);
    CancellationToken cancellationToken = new CancellationToken();
    StockTransactionDetailsLineEditViewModel.Result result = await navigationService.Navigate<StockTransactionDetailsLineEditViewModel, StockTransactionDetailsLineEditViewModel.Params, StockTransactionDetailsLineEditViewModel.Result>(@params, cancellationToken: cancellationToken);
    if (result == null)
      return;
    detailsViewModel.SelectedLine.Quantity = result.Quantity;
    detailsViewModel.SelectedLine.UnitId = result.UnitId;
    detailsViewModel.SelectedLine.Price = result.Price;
    detailsViewModel.SelectedLine.CurrencyId = result.CurrencyId;
  }

  public ICommand SelectedLineDeleteCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnSelectedLineDelete), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

  private void OnSelectedLineDelete()
  {
    this.SelectedLine = this.Details.Lines.RemoveWithSelection<TLine>(this.SelectedLine);
  }

  public ICommand SelectWarehouseCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectWarehouseAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnSelectWarehouseAsync()
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    T obj = detailsViewModel.Details;
    obj.WarehouseId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Warehouse>, string, string>(detailsViewModel.Details.WarehouseId ?? Guid.Empty.ToString());
    obj = default (T);
  }

  public ICommand ShowStockTrackinsList
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnShowStockTrackinsListAsync), (Func<bool>) (() => !this.IsBusy && !this.IsDirty && this.AllowReporting && this.AllowStockTracking()));
    }
  }

  protected virtual bool AllowStockTracking()
  {
    // ISSUE: variable of a boxed type
    __Boxed<T> details = (object) this.Details;
    return details != null && details.IsStockIncome;
  }

  protected virtual Task OnShowStockTrackinsListAsync()
  {
    return this.NavigationService.Navigate<StokTrackingsListViewModel, (string, string)>((this.Details.Id, this.Details.Code));
  }

  public ICommand ImportCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnImportCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  protected virtual async Task OnImportCommandAsync()
  {
    StockTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    IEnumerable<object> source = await detailsViewModel.NavigationService.Navigate<DataImportViewModel, Type, IEnumerable<object>>(typeof (StockTransactionDetailsViewModel<T, TLine>.LineImport));
    int i = 0;
    detailsViewModel.IsBusy = true;
    detailsViewModel.SuspendLoading = true;
    try
    {
      StockTransactionDetailsViewModel<T, TLine>.LineImport[] list = source != null ? source.Cast<StockTransactionDetailsViewModel<T, TLine>.LineImport>().ToArray<StockTransactionDetailsViewModel<T, TLine>.LineImport>() : (StockTransactionDetailsViewModel<T, TLine>.LineImport[]) null;
      if (list != null)
      {
        int itemsCount = list.Length;
        await detailsViewModel.UpdateStocksCacheByCodeAsync(((IEnumerable<StockTransactionDetailsViewModel<T, TLine>.LineImport>) list).Select<StockTransactionDetailsViewModel<T, TLine>.LineImport, string>((Func<StockTransactionDetailsViewModel<T, TLine>.LineImport, string>) (x => x.StockCode)).ToArray<string>());
        await Task.Run((Action) (() =>
        {
          List<TLine> collection = new List<TLine>();
          foreach (StockTransactionDetailsViewModel<T, TLine>.LineImport lineImport in list)
          {
            StockTransactionDetailsViewModel<T, TLine>.LineImport item = lineImport;
            ++i;
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            this.\u003C\u003E4__this.Status = this.\u003C\u003E4__this["Importing {0} of {1} lines", new object[2]
            {
              (object) i,
              (object) itemsCount
            }];
            // ISSUE: reference to a compiler-generated field
            Stock stock = this.\u003C\u003E4__this.StocksCache.Single<Stock>((Func<Stock, bool>) (x => x.Code == item.StockCode));
            string id1 = stock.Units.SingleOrDefault<StockUnit>((Func<StockUnit, bool>) (x => x.Name == item.Unit))?.Id;
            // ISSUE: reference to a compiler-generated field
            string id2 = this.\u003C\u003E4__this.Currencies.List.SingleOrDefault<Currency>((Func<Currency, bool>) (x => x.Name == item.Currency))?.Id;
            // ISSUE: reference to a compiler-generated field
            TLine newLine = this.\u003C\u003E4__this.CreateNewLine(stock, new Decimal?(item.Quantity), id1, new Decimal?(item.Price), id2);
            collection.Add(newLine);
          }
          // ISSUE: reference to a compiler-generated field
          this.\u003C\u003E4__this.Details.Lines = new WatchedObservableCollection<TLine>((IEnumerable<TLine>) collection);
        }));
      }
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    detailsViewModel.Status = (string) null;
    detailsViewModel.SuspendLoading = false;
    detailsViewModel.IsBusy = false;
  }

  public class LineImport
  {
    public string StockCode { get; set; }

    public Decimal Quantity { get; set; }

    public string Unit { get; set; }

    public Decimal Price { get; set; }

    public string Currency { get; set; }
  }
}
