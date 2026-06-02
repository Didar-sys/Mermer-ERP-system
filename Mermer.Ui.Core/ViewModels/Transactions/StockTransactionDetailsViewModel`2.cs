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
    // Приватна змінна для зберігання команди
    private IMvxAsyncCommand _closeCommand;
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
        await base.OnLoad();
        ConnectionSettings configAsync = await Configurator.GetConfigAsync<ConnectionSettings>();
        AllowReporting = configAsync.AllowReporting;

        if (!string.IsNullOrEmpty(ItemId) || !string.IsNullOrEmpty(Details.WarehouseId))
            return;

        Details.WarehouseId = AppSettings.DefaultWarehouseId;
    }

    protected override async Task PostLoad()
    {
        await base.PostLoad();

        Details.StockUnitConverterRequested += StockUnitConverter;

        if (Details.StockUnitConvertions == null)
            Details.StockUnitConvertions = new WatchedObservableCollection<StockUnitConvertion>();

        if (Details.Overheads == null)
            Details.Overheads = new WatchedObservableCollection<StockTransactionOverhead>();

        if (string.IsNullOrEmpty(ItemId) && _stockLineCopies != null)
        {
            await UpdateStocksCacheAsync(_stockLineCopies.Select(x => x.StockId).ToArray());
            foreach (CopyCreateLine stockLineCopy in _stockLineCopies)
            {
                TLine newLineAsync = await CreateNewLineAsync(stockLineCopy.StockId, stockLineCopy.Quantity, stockLineCopy.UnitId, stockLineCopy.Price, stockLineCopy.CurrencyId);
                Details.Lines.Add(newLineAsync);
            }
        }

        StockSearcher.WarehouseId = Details.WarehouseId;
        StockSearcher.CurrencyId = Details.DisplayCurrencyId;
        StockSearcher.ShowLastPurchasePrice = AppSettings.ShowLastPurchasePriceOnSearch;

        await LoadStocksCache();
        Details.RaisePropertyChanged("LineQuantitiesSum");

        // Відновлена лямбда фільтрації складів
        Warehouses.Filter = w => !w.IsDisabled || w.Id == Details.WarehouseId;
    }

    public override async Task Initialize()
    {
        await base.Initialize();

        if (_stockLineCopies == null)
            return;

        _stockLineCopies = null;
        IsDirty = true;
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
        await UpdateStocksCacheAsync(Details.Lines.Select(x => x.StockId).ToArray());
        RaisePropertyChanged(() => StocksCache); // Виправлено explicit non-virtual call
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
        // Відновлена лямбда фільтрації (завантажуємо тільки ті стоки, яких ще немає в кеші)
        string[] array = stockIds.Distinct().Where(id => !StocksCache.Any(sc => sc.Id == id)).ToArray();

        if (!array.Any())
            return;

        foreach (Stock stock in await _stocksRepository.GetListAsync(array))
            StocksCache.Add(stock);
    }

    protected async Task UpdateStocksCacheByCodeAsync(params string[] stockCodes)
    {
        // Відновлена лямбда фільтрації (завантажуємо тільки ті стоки, яких ще немає в кеші)
        string[] stockCodesToAdd = stockCodes.Distinct().Where(code => !StocksCache.Any(sc => sc.Code == code)).ToArray();

        if (!stockCodesToAdd.Any())
        {
            return;
        }

        for (int i = 0; i < stockCodesToAdd.Length; i += 100)
        {
            string[] stockCodesToAddPartial = stockCodesToAdd.Skip(i).Take(100).ToArray();
            var expressionArray = new System.Linq.Expressions.Expression<Func<Stock, bool>>[]
            {
            x => stockCodesToAddPartial.Contains(x.Code)
            };

            foreach (Stock stock in await _stocksRepository.GetAsync(expressionArray))
                StocksCache.Add(stock);
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

    private IMvxAsyncCommand _forceCloseCommand; // Оголошуємо правильне поле

public ICommand ForceCloseCommand
    {
        get
        {
            if (_forceCloseCommand == null)
            {
                _forceCloseCommand = new MvxAsyncCommand(async () =>
                {
                    if (this.IsDirty)
                    {
                        var result = System.Windows.MessageBox.Show(
                            "Ви внесли зміни в документ. Бажаєте закрити вкладку БЕЗ збереження?",
                            "Увага: незбережені дані",
                            System.Windows.MessageBoxButton.YesNo,
                            System.Windows.MessageBoxImage.Warning);

                        if (result == System.Windows.MessageBoxResult.No)
                        {
                            return;
                        }
                    }
                    await this.NavigationService.Close(this);
                });
            }
            return _forceCloseCommand;
        }
    }

    private void DoCloseTransaction()
    {
        // Цей метод відправляє сигнал закриття, який ми ловимо в MainViewPresenter
        this.Close(this);
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
        // Виправлений __Boxed<T>
        return Details != null && Details.IsStockIncome;
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
        var source = await NavigationService.Navigate<DataImportViewModel, Type, IEnumerable<object>>(typeof(LineImport));
        int i = 0;
        IsBusy = true;
        SuspendLoading = true;

        try
        {
            var list = source?.Cast<LineImport>().ToArray();
            if (list != null)
            {
                int itemsCount = list.Length;
                await UpdateStocksCacheByCodeAsync(list.Select(x => x.StockCode).ToArray());

                await Task.Run(() =>
                {
                    var collection = new List<TLine>();
                    foreach (LineImport item in list)
                    {
                        i++;

                        // Використовуємо MvvmCross диспетчер
                        InvokeOnMainThread(() =>
                        {
                            Status = this[$"Importing {i} of {itemsCount} lines"];
                        });

                        Stock stock = StocksCache.Single(x => x.Code == item.StockCode);
                        string id1 = stock.Units.SingleOrDefault(x => x.Name == item.Unit)?.Id;
                        string id2 = Currencies.List.SingleOrDefault(x => x.Name == item.Currency)?.Id;

                        TLine newLine = CreateNewLine(stock, item.Quantity, id1, item.Price, id2);
                        collection.Add(newLine);
                    }

                    InvokeOnMainThread(() =>
                    {
                        Details.Lines = new WatchedObservableCollection<TLine>(collection);
                    });
                });
            }
        }
        catch (Exception ex)
        {
            UserInteractionService.ShowExceptionMessage(ex);
        }

        Status = null;
        SuspendLoading = false;
        IsBusy = false;
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
