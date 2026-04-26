// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning.StockRevisionDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Binyat.Warehousing.Models;
using Payhas.Binyat.Warehousing.Revisioning.Models;
using Payhas.Binyat.Warehousing.Revisioning.Services;
using Payhas.Data;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Services;
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
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning;

public class StockRevisionDetailsViewModel : TransactionDetailsViewModel<StockRevision>
{
  private readonly AppSettings _settings;
  private readonly IStocksRepository _stocksRepository;
  private readonly IStockBalancesRepository _stockBalancesRepository;
  private readonly IRepository<Currency> _currenciesRepository;
  private readonly IRepository<StockSlip> _stockSlipsRepository;
  private readonly CancellationTokenSource _autoReloadCancellation;
  private ObservableCollection<StockRevisionLineInfo> _lines;
  private string _lastSelectedLineId;
  private StockRevisionLineInfo _selectedLine;
  private string _displayCurrencyId;
  private string[] _groupNames;
  private string[] _tagNames;
  private bool _initialized;
  private int _linesCount;
  private Decimal _linesQuantityTotal;
  private bool _pauseLoading;
  private List<Stock> _stocks;
  private List<StockBalance> _stockBalances;
  private bool _isLoaded;
  private IEnumerable<Currency> _currenciesList;
  private List<Stock> _stocksList;

  public StockRevisionDetailsViewModel(
    IConfigurator configurator,
    ILoginService loginService,
    StockSearcher stockSearcher,
    Reference<Currency> currencies,
    Reference<Warehouse> warehouses,
    IStocksRepository stocksRepository,
    IStockBalancesRepository stockBalancesRepository,
    IStockRevisionsRepository repository,
    IRepository<Currency> currenciesRepository,
    IRepository<StockSlip> stockSlipsRepository,
    IListAuthorizer<StockRevision> authorizer,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codeGentor,
    IUserInteractionService userInteractionService)
    : base(codeGentor, (IRepository<StockRevision>) repository, authorizer, loginService, navigationService, userInteractionService)
  {
    this._stocksRepository = stocksRepository;
    this._stockBalancesRepository = stockBalancesRepository;
    this._currenciesRepository = currenciesRepository;
    this._stockSlipsRepository = stockSlipsRepository;
    this.StockSearcher = stockSearcher;
    stockSearcher.ResultSelected += new SearchResultSelected(this.StockSearcher_ResultSelected);
    this.Currencies = currencies;
    this.Warehouses = warehouses;
    this._settings = configurator.GetConfig<AppSettings>();
    this._autoReloadCancellation = new CancellationTokenSource();
  }

  public StockSearcher StockSearcher { get; }

  public Reference<Currency> Currencies { get; }

  public Reference<Warehouse> Warehouses { get; }

  private IStockRevisionsRepository RevisionsRepository
  {
    get => this.Repository as IStockRevisionsRepository;
  }

  public override StockRevision Details
  {
    get => base.Details;
    set
    {
      if (base.Details != null)
        base.Details.PropertyChanged -= new PropertyChangedEventHandler(this.Details_PropertyChanged);
      base.Details = value;
      if (base.Details != null)
        base.Details.PropertyChanged += new PropertyChangedEventHandler(this.Details_PropertyChanged);
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasExceedsSlip));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasDeficitsSlip));
    }
  }

  public bool HasExceedsSlip => !string.IsNullOrEmpty(this.Details?.ExceedSlipId);

  public bool HasDeficitsSlip => !string.IsNullOrEmpty(this.Details?.DeficitSlipId);

  public ObservableCollection<StockRevisionLineInfo> Lines
  {
    get => this._lines;
    set
    {
      this.SetProperty<ObservableCollection<StockRevisionLineInfo>>(ref this._lines, value, nameof (Lines));
    }
  }

  public StockRevisionLineInfo SelectedLine
  {
    get => this._selectedLine;
    set
    {
      this.SetProperty<StockRevisionLineInfo>(ref this._selectedLine, value, nameof (SelectedLine));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsLineSelected));
      if (this.SelectedLine == null)
        return;
      this._lastSelectedLineId = this.SelectedLine.StockRevisionLineId;
    }
  }

  public bool IsLineSelected => this.HasSaveAccess && this.SelectedLine != null;

  public string DisplayCurrencyId
  {
    get => this._displayCurrencyId;
    set
    {
      if (this.SetProperty<string>(ref this._displayCurrencyId, value, nameof (DisplayCurrencyId)) && !this.IsBusy && this.Lines != null)
      {
        foreach (RequestCurrencyConverter line in (Collection<StockRevisionLineInfo>) this.Lines)
          line.UpdateDisplayCurrencyId(false);
      }
      this.StockSearcher.CurrencyId = this._displayCurrencyId;
    }
  }

  public virtual string[] GroupNames
  {
    get => this._groupNames;
    set => this.SetProperty<string[]>(ref this._groupNames, value, nameof (GroupNames));
  }

  public virtual string[] TagNames
  {
    get => this._tagNames;
    set => this.SetProperty<string[]>(ref this._tagNames, value, nameof (TagNames));
  }

  public virtual bool IsFinished
  {
    get
    {
      StockRevision details = this.Details;
      return details != null && details.FinishDate.HasValue;
    }
  }

  protected virtual async Task LoadFacetsAsync()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    Dictionary<string, Dictionary<string, int>> facets = await ((IRepositoryWithFacets<StockRevision>) detailsViewModel.Repository).GetFacets("GroupNames", "TagNames");
    detailsViewModel.GroupNames = facets["GroupNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
    detailsViewModel.TagNames = facets["TagNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }

  protected override async Task PreLoad()
  {
    await Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync(), this.Currencies.Initialize(), this.Warehouses.Initialize(), this.StockSearcher.Initialize());
    if (this._initialized)
      return;
    this.DisplayCurrencyId = this.Currencies.List.Single<Currency>((Func<Currency, bool>) (x => x.IsDefault)).Id;
    this._initialized = true;
  }

  protected override async Task OnLoad()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    if (string.IsNullOrEmpty(detailsViewModel.ItemId))
      throw new Exception("Wrong usage, revision must first be created!");
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__1();
    detailsViewModel._stocks = new List<Stock>();
    detailsViewModel._stockBalances = new List<StockBalance>();
    await detailsViewModel.LoadLines();
    detailsViewModel.StockSearcher.WarehouseId = detailsViewModel.Details.WarehouseId;
    detailsViewModel.StockSearcher.CurrencyId = detailsViewModel.DisplayCurrencyId;
  }

  protected virtual async Task LoadLines()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    if (detailsViewModel._pauseLoading)
      return;
    StockRevisionLine[] array1 = (await detailsViewModel.RevisionsRepository.GetLinesAsync(detailsViewModel.Details.Id)).ToArray<StockRevisionLine>();
    int length = array1.Length;
    Decimal num = ((IEnumerable<StockRevisionLine>) array1).Sum<StockRevisionLine>((Func<StockRevisionLine, Decimal>) (x => x.Quantity));
    if (detailsViewModel._linesCount == length && detailsViewModel._linesQuantityTotal == num)
      return;
    detailsViewModel._linesCount = length;
    detailsViewModel._linesQuantityTotal = num;
    StockRevisionLineInfo[] array2 = (await detailsViewModel.RevisionsRepository.CalcLineInfosAsync(detailsViewModel.Details, (IEnumerable<StockRevisionLine>) array1, new Func<string[], Task<IEnumerable<Stock>>>(detailsViewModel.StocksGetter), new Func<string[], Task<IEnumerable<StockBalance>>>(detailsViewModel.StockBalancesGetter), new Func<(string, DateTime?)[], Task<IEnumerable<StockBalance>>>(detailsViewModel.StockBalancesGetterAlt), detailsViewModel.DisplayCurrencyId)).ToArray<StockRevisionLineInfo>();
    foreach (StockRevisionLineInfo revisionLineInfo in array2)
    {
      revisionLineInfo.DisplayCurrencyIdRequested += new CurrencyId(detailsViewModel.GetDisplayCurrencyId);
      revisionLineInfo.CurrencyConverterRequested += new CurrencyConverter(detailsViewModel.GetCurrencyConverter);
    }
    detailsViewModel.Lines = new ObservableCollection<StockRevisionLineInfo>((IEnumerable<StockRevisionLineInfo>) array2);
    if (string.IsNullOrEmpty(detailsViewModel._lastSelectedLineId))
      return;
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.SelectedLine = detailsViewModel.Lines.FirstOrDefault<StockRevisionLineInfo>(new Func<StockRevisionLineInfo, bool>(detailsViewModel.\u003CLoadLines\u003Eb__57_1));
  }

  private async Task<IEnumerable<Stock>> StocksGetter(string[] stockIds)
  {
    IEnumerable<string> existingStockIds = this._stocks.Select<Stock, string>((Func<Stock, string>) (x => x.Id));
    string[] array = ((IEnumerable<string>) stockIds).Where<string>((Func<string, bool>) (x => !existingStockIds.Contains<string>(x))).ToArray<string>();
    if (((IEnumerable<string>) array).Any<string>())
      this._stocks.AddRange(await this._stocksRepository.GetAsync(array));
    return (IEnumerable<Stock>) this._stocks;
  }

  private async Task<IEnumerable<StockBalance>> StockBalancesGetter(string[] stockIds)
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    IEnumerable<string> existingStockIds = detailsViewModel._stockBalances.Select<StockBalance, string>((Func<StockBalance, string>) (x => x.StockId));
    string[] array = ((IEnumerable<string>) stockIds).Where<string>((Func<string, bool>) (x => !existingStockIds.Contains<string>(x))).ToArray<string>();
    if (((IEnumerable<string>) array).Any<string>())
    {
      IEnumerable<StockBalance> async = await detailsViewModel._stockBalancesRepository.GetAsync(detailsViewModel.Details.WarehouseId, array, detailsViewModel.Details.FinishDate);
      detailsViewModel._stockBalances.AddRange(async);
    }
    return (IEnumerable<StockBalance>) detailsViewModel._stockBalances;
  }

  private async Task<IEnumerable<StockBalance>> StockBalancesGetterAlt(
    (string stockId, DateTime? balanceDate)[] stockBalanceDates)
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    IEnumerable<string> existingStockIds = detailsViewModel._stockBalances.Select<StockBalance, string>((Func<StockBalance, string>) (x => x.StockId));
    (string, DateTime?)[] array = ((IEnumerable<(string, DateTime?)>) stockBalanceDates).Where<(string, DateTime?)>((Func<(string, DateTime?), bool>) (x => !existingStockIds.Contains<string>(x.stockId))).ToArray<(string, DateTime?)>();
    if (((IEnumerable<(string, DateTime?)>) array).Any<(string, DateTime?)>())
    {
      IEnumerable<StockBalance> async = await detailsViewModel._stockBalancesRepository.GetAsync(detailsViewModel.Details.WarehouseId, array);
      detailsViewModel._stockBalances.AddRange(async);
    }
    return (IEnumerable<StockBalance>) detailsViewModel._stockBalances;
  }

  protected override async Task PostLoad()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__2();
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Warehouses.Filter = new Func<Warehouse, bool>(detailsViewModel.\u003CPostLoad\u003Eb__64_0);
    if (detailsViewModel._isLoaded)
      return;
    detailsViewModel.AutoReloadLines(detailsViewModel._autoReloadCancellation.Token);
    detailsViewModel._isLoaded = true;
  }

  private string GetDisplayCurrencyId() => this.DisplayCurrencyId;

  private CurrencyConvertion GetCurrencyConverter(string currencyId)
  {
    Currency currency = this.Currencies.List.Single<Currency>((Func<Currency, bool>) (x => x.Id == currencyId));
    CurrencyRate rate = currency.GetRate(this.Details.FinishDate);
    return new CurrencyConvertion()
    {
      CurrencyId = currency.Id,
      Multiplier = rate.Multiplier,
      Divider = rate.Divider
    };
  }

  private async void AutoReloadLines(CancellationToken cancellationToken)
  {
    try
    {
      await Task.Run((Func<Task>) (async () =>
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          if (!this.IsBusy)
          {
            try
            {
              await this.LoadLines();
            }
            catch (Exception ex)
            {
            }
          }
          await Task.Delay(TimeSpan.FromSeconds(1.0), cancellationToken);
        }
      }), cancellationToken);
    }
    catch (Exception ex)
    {
    }
  }

  private DateTime? GetFirstCountDate(string stockId)
  {
    if (this.Lines != null)
    {
      IEnumerable<StockRevisionLineInfo> source = this.Lines.Where<StockRevisionLineInfo>((Func<StockRevisionLineInfo, bool>) (x => x.StockId == stockId));
      if (source.Any<StockRevisionLineInfo>())
        return new DateTime?(source.Min<StockRevisionLineInfo, DateTime>((Func<StockRevisionLineInfo, DateTime>) (x => x.Date)));
    }
    return new DateTime?();
  }

  private async void StockSearcher_ResultSelected(StockSearcher searcher, StockSearchResult result)
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    try
    {
      Stock stock = await detailsViewModel._stocksRepository.GetAsync(result.Id);
      StockPrice stockPrice = stock.GetPrice(detailsViewModel.Details.FinishDate);
      StockRevisionCountInfo countInfoAsync = await detailsViewModel.RevisionsRepository.GetCountInfoAsync(detailsViewModel.Details.Id, stock.Id, new Func<string, DateTime?>(detailsViewModel.GetFirstCountDate));
      StockRevisionDetailsLineEditViewModel.Params params1 = new StockRevisionDetailsLineEditViewModel.Params();
      params1.StockCode = stock.Code;
      params1.StockName = stock.Name;
      params1.IsPriceReadonly = !detailsViewModel._settings.AllowStockPriceChangeOnRevision;
      params1.Currencies = detailsViewModel.Currencies.List;
      params1.CurrencyId = stockPrice.CurrencyId;
      params1.Price = stockPrice.Price;
      params1.Units = (IEnumerable<StockUnit>) stock.Units;
      params1.UnitId = stock.UnitId;
      params1.PreviousCounted = countInfoAsync.TotalCounted;
      params1.TotalComputed = countInfoAsync.TotalComputed;
      StockRevisionDetailsLineEditViewModel.Params params2 = params1;
      StockRevisionDetailsLineEditViewModel.Result result1 = await detailsViewModel.NavigationService.Navigate<StockRevisionDetailsLineEditViewModel, StockRevisionDetailsLineEditViewModel.Params, StockRevisionDetailsLineEditViewModel.Result>(params2);
      if (result1 == null)
        return;
      StockRevisionLine stockRevisionLine = new StockRevisionLine();
      stockRevisionLine.Id = Guid.NewGuid().ToString();
      stockRevisionLine.StockRevisionId = detailsViewModel.Details.Id;
      stockRevisionLine.StockId = stock.Id;
      stockRevisionLine.Date = DateTime.Now;
      stockRevisionLine.Quantity = result1.Quantity;
      stockRevisionLine.UnitId = result1.UnitId;
      stockRevisionLine.UserId = detailsViewModel.LoginService.Session.UserId;
      stockRevisionLine.UserName = detailsViewModel.LoginService.Session.Username;
      StockRevisionLine line = stockRevisionLine;
      if (detailsViewModel._settings.AllowStockPriceChangeOnRevision)
      {
        line.Price = new Decimal?(result1.Price);
        line.CurrencyId = result1.CurrencyId;
      }
      detailsViewModel._lastSelectedLineId = line.Id;
      await detailsViewModel.RevisionsRepository.StoreLineAsync(line);
      stock = (Stock) null;
      stockPrice = (StockPrice) null;
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
  }

  private void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "ExceedSlipId")
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasExceedsSlip));
    if (!(e.PropertyName == "DeficitSlipId"))
      return;
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasDeficitsSlip));
  }

  public override void Dispose()
  {
    this._autoReloadCancellation.Cancel();
    this._autoReloadCancellation.Dispose();
    base.Dispose();
  }

  public ICommand SelectedLineEditCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectedLineEditCommandAsync), (Func<bool>) (() => !this.IsBusy && this.IsLineSelected));
    }
  }

  protected virtual async Task OnSelectedLineEditCommandAsync()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    try
    {
      Stock async = await detailsViewModel._stocksRepository.GetAsync(detailsViewModel.SelectedLine.StockId);
      StockRevisionDetailsLineEditViewModel.Params params1 = new StockRevisionDetailsLineEditViewModel.Params();
      params1.StockCode = async.Code;
      params1.StockName = async.Name;
      params1.IsPriceReadonly = !detailsViewModel._settings.AllowStockPriceChangeOnRevision;
      params1.Currencies = detailsViewModel.Currencies.List;
      params1.CurrencyId = detailsViewModel.SelectedLine.StockPriceCurrencyId;
      params1.Price = detailsViewModel.SelectedLine.StockPrice;
      params1.Units = (IEnumerable<StockUnit>) async.Units;
      params1.Quantity = detailsViewModel.SelectedLine.Quantity;
      params1.UnitId = detailsViewModel.SelectedLine.UnitId;
      params1.PreviousCounted = detailsViewModel.SelectedLine.TotalCounted - detailsViewModel.SelectedLine.CurrentCounted;
      params1.TotalComputed = detailsViewModel.SelectedLine.TotalComputed;
      StockRevisionDetailsLineEditViewModel.Params params2 = params1;
      StockRevisionDetailsLineEditViewModel.Result editResult = await detailsViewModel.NavigationService.Navigate<StockRevisionDetailsLineEditViewModel, StockRevisionDetailsLineEditViewModel.Params, StockRevisionDetailsLineEditViewModel.Result>(params2);
      if (editResult == null)
        return;
      StockRevisionLine lineAsync = await detailsViewModel.RevisionsRepository.GetLineAsync(detailsViewModel.SelectedLine.StockRevisionLineId);
      lineAsync.Quantity = editResult.Quantity;
      lineAsync.UnitId = editResult.UnitId;
      if (detailsViewModel._settings.AllowStockPriceChangeOnRevision)
      {
        lineAsync.Price = new Decimal?(editResult.Price);
        lineAsync.CurrencyId = editResult.CurrencyId;
      }
      await detailsViewModel.RevisionsRepository.StoreLineAsync(lineAsync);
      editResult = (StockRevisionDetailsLineEditViewModel.Result) null;
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
  }

  public ICommand SelectedLineDeleteCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectedLineDeleteCommandAsync), (Func<bool>) (() => !this.IsBusy && this.IsLineSelected));
    }
  }

  protected virtual async Task OnSelectedLineDeleteCommandAsync()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    try
    {
      if (!detailsViewModel.UserInteractionService.ShowMessage(detailsViewModel["Deleting Revision Line", Array.Empty<object>()], detailsViewModel["Are you sure to delete selected line?", Array.Empty<object>()], UserInteractionType.YesNo).GetValueOrDefault())
        return;
      await detailsViewModel.RevisionsRepository.DeleteLineAsync(detailsViewModel.SelectedLine.StockRevisionLineId);
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
  }

  public ICommand ShowUncountedCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnShowUncountedCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && !this.IsFinished));
    }
  }

  protected virtual async Task OnShowUncountedCommandAsync()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    detailsViewModel._pauseLoading = true;
    await detailsViewModel.NavigationService.Navigate<StockRevisionDetailsUncountedViewModel, string>(detailsViewModel.Details.Id);
    detailsViewModel._pauseLoading = false;
  }

  public ICommand ShowReportCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnShowReportCommandAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual Task OnShowReportCommandAsync()
  {
    return this.NavigationService.Navigate<StockRevisionDetailsReportViewModel, Tuple<string, DateTime?>>(new Tuple<string, DateTime?>(this.Details.Id, this.Details.FinishDate));
  }

  public ICommand CreateRevisionExceedsSlipCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateRevisionExceedsSlipCommandAsync), (Func<bool>) (() => !this.IsBusy && !this.HasExceedsSlip && !this.IsDirty && this.IsFinished));
    }
  }

  protected virtual async Task OnCreateRevisionExceedsSlipCommandAsync()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    detailsViewModel.IsBusy = true;
    detailsViewModel.Status = detailsViewModel["Creating new revision exceeds slip...", Array.Empty<object>()];
    try
    {
      IEnumerable<StockRevisionDetailsViewModel.ItemsExtracted> items = StockRevisionDetailsViewModel.ExtractItems(detailsViewModel.Lines.Where<StockRevisionLineInfo>((Func<StockRevisionLineInfo, bool>) (x => x.IsExceed)), 1);
      StockSlip slip = await detailsViewModel.GetStockSlip(StockSlipType.RevisionExceed, items);
      await detailsViewModel._stockSlipsRepository.CreateAsync(slip);
      detailsViewModel.Details.ExceedSlipId = slip.Id;
      int num = await detailsViewModel.OnSaveAsync() ? 1 : 0;
      slip = (StockSlip) null;
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    detailsViewModel.Status = (string) null;
    detailsViewModel.IsBusy = false;
  }

  public ICommand UpdateRevisionExceedsSlipCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnUpdateRevisionExceedsSlipCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasExceedsSlip && !this.IsDirty && this.IsFinished));
    }
  }

  protected virtual async Task OnUpdateRevisionExceedsSlipCommandAsync()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    detailsViewModel.IsBusy = true;
    detailsViewModel.Status = detailsViewModel["Updating revision exceeds slip...", Array.Empty<object>()];
    try
    {
      IEnumerable<StockRevisionDetailsViewModel.ItemsExtracted> items = StockRevisionDetailsViewModel.ExtractItems(detailsViewModel.Lines.Where<StockRevisionLineInfo>((Func<StockRevisionLineInfo, bool>) (x => x.IsExceed)), 1);
      StockSlip stockSlip = await detailsViewModel.GetStockSlip(StockSlipType.RevisionExceed, items, detailsViewModel.Details.ExceedSlipId);
      await detailsViewModel._stockSlipsRepository.UpdateAsync(stockSlip);
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    detailsViewModel.Status = (string) null;
    detailsViewModel.IsBusy = false;
  }

  public ICommand OpenRevisionExceedsSlipCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnOpenRevisionExceedsSlipCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasExceedsSlip));
    }
  }

  protected virtual Task OnOpenRevisionExceedsSlipCommandAsync()
  {
    return this.NavigationService.Navigate<StockSlipDetailsViewModel, string>(this.Details.ExceedSlipId);
  }

  public ICommand CreateRevisionDeficitsSlipCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateRevisionDeficitsSlipCommandAsync), (Func<bool>) (() => !this.IsBusy && !this.HasDeficitsSlip && !this.IsDirty && this.IsFinished));
    }
  }

  protected virtual async Task OnCreateRevisionDeficitsSlipCommandAsync()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    detailsViewModel.IsBusy = true;
    detailsViewModel.Status = detailsViewModel["Creating new revision deficits slip...", Array.Empty<object>()];
    try
    {
      IEnumerable<StockRevisionDetailsViewModel.ItemsExtracted> items = StockRevisionDetailsViewModel.ExtractItems(detailsViewModel.Lines.Where<StockRevisionLineInfo>((Func<StockRevisionLineInfo, bool>) (x => x.IsDeficit)), -1);
      StockSlip slip = await detailsViewModel.GetStockSlip(StockSlipType.RevisionDeficit, items);
      await detailsViewModel._stockSlipsRepository.CreateAsync(slip);
      detailsViewModel.Details.DeficitSlipId = slip.Id;
      int num = await detailsViewModel.OnSaveAsync() ? 1 : 0;
      slip = (StockSlip) null;
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    detailsViewModel.Status = (string) null;
    detailsViewModel.IsBusy = false;
  }

  public ICommand UpdateRevisionDeficitsSlipCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnUpdateRevisionDeficitsSlipCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasDeficitsSlip && !this.IsDirty && this.IsFinished));
    }
  }

  protected virtual async Task OnUpdateRevisionDeficitsSlipCommandAsync()
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    detailsViewModel.IsBusy = true;
    detailsViewModel.Status = detailsViewModel["Updating revision deficits slip...", Array.Empty<object>()];
    try
    {
      IEnumerable<StockRevisionDetailsViewModel.ItemsExtracted> items = StockRevisionDetailsViewModel.ExtractItems(detailsViewModel.Lines.Where<StockRevisionLineInfo>((Func<StockRevisionLineInfo, bool>) (x => x.IsDeficit)), -1);
      StockSlip stockSlip = await detailsViewModel.GetStockSlip(StockSlipType.RevisionDeficit, items, detailsViewModel.Details.DeficitSlipId);
      await detailsViewModel._stockSlipsRepository.UpdateAsync(stockSlip);
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    detailsViewModel.Status = (string) null;
    detailsViewModel.IsBusy = false;
  }

  public ICommand OpenRevisionDeficitsSlipCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnOpenRevisionDeficitsSlipCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasDeficitsSlip));
    }
  }

  protected virtual Task OnOpenRevisionDeficitsSlipCommandAsync()
  {
    return this.NavigationService.Navigate<StockSlipDetailsViewModel, string>(this.Details.DeficitSlipId);
  }

  private static IEnumerable<StockRevisionDetailsViewModel.ItemsExtracted> ExtractItems(
    IEnumerable<StockRevisionLineInfo> list,
    int multiplier)
  {
    return list.GroupBy(x => new
    {
      StockId = x.StockId,
      StockPrice = x.StockPrice,
      StockPriceCurrencyId = x.StockPriceCurrencyId
    }).Select<IGrouping<\u003C\u003Ef__AnonymousType6<string, Decimal, string>, StockRevisionLineInfo>, StockRevisionDetailsViewModel.ItemsExtracted>(g => new StockRevisionDetailsViewModel.ItemsExtracted()
    {
      StockId = g.Key.StockId,
      Price = g.Key.StockPrice,
      CurrencyId = g.Key.StockPriceCurrencyId,
      Quantity = g.First<StockRevisionLineInfo>().TotalDifference * (Decimal) multiplier,
      UnitId = g.First<StockRevisionLineInfo>().UnitId
    });
  }

  private async Task<StockSlip> GetStockSlip(
    StockSlipType type,
    IEnumerable<StockRevisionDetailsViewModel.ItemsExtracted> items,
    string id = null)
  {
    StockRevisionDetailsViewModel detailsViewModel = this;
    IEnumerable<Currency> async = await detailsViewModel._currenciesRepository.GetAsync();
    detailsViewModel._currenciesList = async;
    if (!(items is StockRevisionDetailsViewModel.ItemsExtracted[] itemsExtractedArray))
      itemsExtractedArray = items.ToArray<StockRevisionDetailsViewModel.ItemsExtracted>();
    StockRevisionDetailsViewModel.ItemsExtracted[] itemsArray = itemsExtractedArray;
    await detailsViewModel.UpdateStocksCacheAsync(((IEnumerable<StockRevisionDetailsViewModel.ItemsExtracted>) itemsArray).Select<StockRevisionDetailsViewModel.ItemsExtracted, string>((Func<StockRevisionDetailsViewModel.ItemsExtracted, string>) (x => x.StockId)).ToArray<string>());
    StockSlip stockSlip1 = new StockSlip();
    stockSlip1.SlipType = type;
    stockSlip1.IsCompleted = true;
    stockSlip1.Date = detailsViewModel.Details.FinishDate.Value.AddMinutes(1.0);
    stockSlip1.Id = id ?? Guid.NewGuid().ToString();
    stockSlip1.WarehouseId = detailsViewModel.Details.WarehouseId;
    stockSlip1.UserId = detailsViewModel.LoginService.Session.UserId;
    stockSlip1.UserName = detailsViewModel.LoginService.Session.Username;
    StockSlip stockSlip2 = stockSlip1;
    stockSlip2.Code = await detailsViewModel.CodeGenerationService.GetNextCode();
    stockSlip1.Description = detailsViewModel["Auto created from stock revision: {0}", new object[1]
    {
      (object) detailsViewModel.Details.Code
    }];
    stockSlip1.Lines = new WatchedObservableCollection<StockSlipLine>();
    stockSlip1.CurrencyConvertions = new WatchedObservableCollection<CurrencyConvertion>();
    stockSlip1.StockUnitConvertions = new WatchedObservableCollection<StockUnitConvertion>();
    StockSlip stockSlip3 = stockSlip1;
    stockSlip2 = (StockSlip) null;
    stockSlip1 = (StockSlip) null;
    stockSlip3.DefaultCurrencyIdRequested += new CurrencyId(detailsViewModel.Slip_DefaultCurrencyIdRequested);
    stockSlip3.CurrencyConverterRequested += new CurrencyConverter(detailsViewModel.Slip_CurrencyConverterRequested);
    stockSlip3.StockUnitConverterRequested += new StockUnitConverter(detailsViewModel.Slip_StockUnitConverterRequested);
    foreach (StockRevisionDetailsViewModel.ItemsExtracted itemsExtracted in itemsArray)
    {
      StockSlipLine stockSlipLine = new StockSlipLine();
      stockSlipLine.Id = Guid.NewGuid().ToString();
      stockSlipLine.StockId = itemsExtracted.StockId;
      stockSlipLine.Price = itemsExtracted.Price;
      stockSlipLine.CurrencyId = itemsExtracted.CurrencyId;
      stockSlipLine.Quantity = Math.Round(itemsExtracted.Quantity, 2);
      stockSlipLine.UnitId = itemsExtracted.UnitId;
      StockSlipLine line = stockSlipLine;
      if (stockSlip3.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != line.CurrencyId)))
        stockSlip3.CurrencyConvertions.Add(detailsViewModel.Slip_CurrencyConverterRequested(line.CurrencyId));
      if (stockSlip3.StockUnitConvertions.All<StockUnitConvertion>((Func<StockUnitConvertion, bool>) (x => x.StockId != line.StockId || x.UnitId != line.UnitId)))
        stockSlip3.StockUnitConvertions.Add(detailsViewModel.Slip_StockUnitConverterRequested(line.StockId, line.UnitId));
      stockSlip3.Lines.Add(line);
    }
    StockSlip stockSlip4 = stockSlip3;
    itemsArray = (StockRevisionDetailsViewModel.ItemsExtracted[]) null;
    return stockSlip4;
  }

  private string Slip_DefaultCurrencyIdRequested()
  {
    return this._currenciesList.Single<Currency>((Func<Currency, bool>) (x => x.IsDefault)).Id;
  }

  private CurrencyConvertion Slip_CurrencyConverterRequested(string currencyId)
  {
    CurrencyRate rate = this._currenciesList.Single<Currency>((Func<Currency, bool>) (x => x.Id == currencyId)).GetRate(this.Details.FinishDate);
    return new CurrencyConvertion()
    {
      CurrencyId = currencyId,
      Multiplier = rate.Multiplier,
      Divider = rate.Divider
    };
  }

  private StockUnitConvertion Slip_StockUnitConverterRequested(string stockId, string unitId)
  {
    StockUnit stockUnit = this._stocksList.Single<Stock>((Func<Stock, bool>) (x => x.Id == stockId)).Units.Single<StockUnit>((Func<StockUnit, bool>) (x => x.Id == unitId));
    return new StockUnitConvertion()
    {
      StockId = stockId,
      UnitId = unitId,
      Multiplier = stockUnit.Multiplier,
      Divider = stockUnit.Divider
    };
  }

  protected async Task UpdateStocksCacheAsync(params string[] stockIds)
  {
    this._stocksList = new List<Stock>();
    if (!((IEnumerable<string>) ((IEnumerable<string>) stockIds).Distinct<string>().ToArray<string>()).Any<string>())
      return;
    List<Stock> stockList = this._stocksList;
    stockList.AddRange(await this._stocksRepository.GetListAsync(stockIds));
    stockList = (List<Stock>) null;
  }

  private class ItemsExtracted
  {
    public string StockId { get; set; }

    public Decimal Quantity { get; set; }

    public string UnitId { get; set; }

    public Decimal Price { get; set; }

    public string CurrencyId { get; set; }
  }
}
