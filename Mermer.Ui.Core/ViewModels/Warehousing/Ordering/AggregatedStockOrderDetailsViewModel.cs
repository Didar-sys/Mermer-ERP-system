// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.Ordering.AggregatedStockOrderDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Mermer.Authorization.Services;
using Mermer.Common.Models;
using Mermer.Common.Settings;
using Mermer.CRM.Models;
using Mermer.Data;
using Mermer.Data.Authorizers;
using Mermer.Data.Extenders;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Warehousing.Ordering.Services;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
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
namespace Mermer.Ui.Core.ViewModels.Warehousing.Ordering;

public class AggregatedStockOrderDetailsViewModel : 
  TransactionDetailsViewModel<AggregatedStockOrder>,
  IMvxViewModel<AggregatedStockOrderDetailsViewModel.Params>,
  IMvxViewModel
{
  private readonly IConfigurator _configurator;
  private readonly IStockBalancesRepository _stockBalancesRepository;
  private readonly IStockOrderActionsRepository _orderActionsRepository;
  private readonly IRepository<Stock> _stocksRepository;
  private ObservableCollection<Stock> _stocksCache;
  private ObservableCollection<ListHelper<string, Decimal>> _stocksBalances;
  private ColumnDescription[] _columns;
  private AggregatedStockOrderLine _selectedLine;
  private string[] _groupNames;
  private string[] _tagNames;
  private AggregatedStockOrderDetailsViewModel.Params _paramenter;

  public AggregatedStockOrderDetailsViewModel(
    IConfigurator configurator,
    ILoginService loginService,
    StockSearcher stockSearcher,
    Reference<Warehouse> warehouses,
    Reference<Partner> partners, // ДОБАВЛЕНО
    IRepository<Stock> stocksRepository,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IRepository<AggregatedStockOrder> repository,
    IListAuthorizer<AggregatedStockOrder> authorizer,
    IUserInteractionService userInteractionService,
    IStockBalancesRepository stockBalancesRepository,
    IStockOrderActionsRepository orderActionsRepository)
    : base(codegentor, repository, authorizer, loginService, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._stocksRepository = stocksRepository;
    this._stockBalancesRepository = stockBalancesRepository;
    this._orderActionsRepository = orderActionsRepository;
    this.Warehouses = warehouses;
    this.Partners = partners; // ДОБАВЛЕНО
    this.StockSearcher = stockSearcher;
    this.StockSearcher.ResultSelected += new SearchResultSelected(this.StockSearcher_ResultSelected);
  }

  public StockSearcher StockSearcher { get; }

  public Reference<Warehouse> Warehouses { get; }

  public Reference<Partner> Partners { get; set; }
    public ICommand SelectPartnerCommand
    {
        get
        {
            return new MvxAsyncCommand(SelectPartnerAsync, () => !this.IsBusy && this.HasSaveAccess);
        }
    }

    private async Task SelectPartnerAsync()
    {
        Details.PartnerId = await NavigationService.Navigate<ListViewModel<Partner>, string, string>(Details.PartnerId);
    }

    public ObservableCollection<Stock> StocksCache
  {
    get => this._stocksCache;
    set
    {
      this.SetProperty<ObservableCollection<Stock>>(ref this._stocksCache, value, nameof (StocksCache));
    }
  }

  public ObservableCollection<ListHelper<string, Decimal>> StocksBalances
  {
    get => this._stocksBalances;
    set
    {
      this.SetProperty<ObservableCollection<ListHelper<string, Decimal>>>(ref this._stocksBalances, value, nameof (StocksBalances));
    }
  }

  public ColumnDescription[] Columns
  {
    get => this._columns;
    set => this.SetProperty<ColumnDescription[]>(ref this._columns, value, nameof (Columns));
  }

  public override AggregatedStockOrder Details
  {
    get => base.Details;
    set
    {
      if (base.Details != null)
        base.Details.PropertyChanged -= new PropertyChangedEventHandler(this.Details_PropertyChanged);
      base.Details = value;
      if (base.Details == null)
        return;
      base.Details.PropertyChanged += new PropertyChangedEventHandler(this.Details_PropertyChanged);
    }
  }

  public AggregatedStockOrderLine SelectedLine
  {
    get => this._selectedLine;
    set
    {
      this.SetProperty<AggregatedStockOrderLine>(ref this._selectedLine, value, nameof (SelectedLine));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsLineSelected));
    }
  }

  public bool IsLineSelected => this.SelectedLine != null;

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

  protected virtual async Task LoadFacetsAsync()
  {
    AggregatedStockOrderDetailsViewModel detailsViewModel = this;
    Dictionary<string, Dictionary<string, int>> facets = await ((IRepositoryWithFacets<AggregatedStockOrder>) detailsViewModel.Repository).GetFacets("GroupNames", "TagNames");
    detailsViewModel.GroupNames = facets["GroupNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
    detailsViewModel.TagNames = facets["TagNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }

  private async void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    AggregatedStockOrderDetailsViewModel detailsViewModel = this;
    if (!(e.PropertyName == "WarehouseId"))
      return;
    await detailsViewModel.UpdateStockBalances();
    detailsViewModel.StockSearcher.WarehouseId = detailsViewModel.Details.WarehouseId;
  }

  public void Prepare(
    AggregatedStockOrderDetailsViewModel.Params parameter)
  {
    this._paramenter = parameter;
  }

  protected override Task PreLoad()
  {
    this.StocksCache = new ObservableCollection<Stock>();
    this.StocksBalances = new ObservableCollection<ListHelper<string, Decimal>>();
    return Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync(), this.Warehouses.Initialize(), this.Partners.Initialize(), this.StockSearcher.Initialize());
  }

    protected override async Task PostLoad()
    {
        await base.PostLoad();

        if (Details.Lines == null)
            Details.Lines = new WatchedObservableCollection<AggregatedStockOrderLine>();

        if (string.IsNullOrEmpty(ItemId))
        {
            if (_paramenter != null)
            {
                Details.WarehouseId = _paramenter.WarehouseId;
                foreach (string stockId in _paramenter.StockIds)
                {
                    AggregatedStockOrderLine line = await CreateLine(stockId);
                    Details.Lines.Add(line);
                }
            }

            if (string.IsNullOrEmpty(Details.WarehouseId))
            {
                AppSettings configAsync = await _configurator.GetConfigAsync<AppSettings>();
                Details.WarehouseId = configAsync.DefaultWarehouseId;
            }
        }

        StockSearcher.WarehouseId = Details.WarehouseId;
        await LoadStocksCache();

        Warehouses.Filter = w => !w.IsDisabled || w.Id == Details.WarehouseId;
        Partners.Filter = p => !p.IsDisabled || p.Id == Details.PartnerId; // <--- ДОБАВЛЕНО

        GenerateColumns();
    }

    private void GenerateColumns()
  {
    this.Columns = this.Warehouses.List.Select<Warehouse, ColumnDescription>((Func<Warehouse, ColumnDescription>) (x => new ColumnDescription(x.Id, x.Name))).ToArray<ColumnDescription>();
  }

    private async Task LoadStocksCache()
    {
        foreach (AggregatedStockOrderLine line in Details.Lines)
        {
            Stock stocksCacheAsync = await GetFromStocksCacheAsync(line.StockId);
        }

       
        RaisePropertyChanged(() => StocksCache);
    }

    protected Stock GetFromStocksCache(string stockId)
  {
    return this.GetFromStocksCacheAsync(stockId).GetAwaiter().GetResult();
  }

  protected async Task<Stock> GetFromStocksCacheAsync(string stockId)
  {
    Stock stock = this.StocksCache.SingleOrDefault<Stock>((Func<Stock, bool>) (x => x.Id == stockId));
    if (stock == null)
    {
      stock = await this._stocksRepository.GetAsync(stockId);
      this.StocksCache.Add(stock);
    }
    await this.UpdateStockBalance(stockId);
    Stock stocksCacheAsync = stock;
    stock = (Stock) null;
    return stocksCacheAsync;
  }

  protected async Task UpdateStockBalances()
  {
    foreach (Model model in (Collection<Stock>) this.StocksCache)
      await this.UpdateStockBalance(model.Id);
  }

  protected async Task UpdateStockBalance(string stockId)
  {
    AggregatedStockOrderDetailsViewModel detailsViewModel = this;
    Decimal num = (await detailsViewModel._stockBalancesRepository.GetAsync(stockId, DateTime.Now, detailsViewModel.Details.WarehouseId)).Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Balance));
    ListHelper<string, Decimal> listHelper = detailsViewModel.StocksBalances.SingleOrDefault<ListHelper<string, Decimal>>((Func<ListHelper<string, Decimal>, bool>) (x => x.Key == stockId));
    if (listHelper == null)
    {
      listHelper = new ListHelper<string, Decimal>()
      {
        Key = stockId
      };
      detailsViewModel.StocksBalances.Add(listHelper);
    }
    listHelper.Value = num;
  }

  private async void StockSearcher_ResultSelected(StockSearcher searcher, StockSearchResult result)
  {
    AggregatedStockOrderDetailsViewModel detailsViewModel = this;
    if (detailsViewModel.Details.Lines.Any<AggregatedStockOrderLine>((Func<AggregatedStockOrderLine, bool>) (x => x.StockId == result.Id)))
      return;
    AggregatedStockOrderLine line = await detailsViewModel.CreateLine(result.Id);
    detailsViewModel.Details.Lines.Add(line);
    detailsViewModel.SelectedLine = line;
  }

  private async Task<AggregatedStockOrderLine> CreateLine(string stockId)
  {
    Stock stocksCacheAsync = await this.GetFromStocksCacheAsync(stockId);
    AggregatedStockOrderLine line = new AggregatedStockOrderLine()
    {
      StockId = stocksCacheAsync.Id,
      UnitId = stocksCacheAsync.UnitId
    };
    line.Orders = new WatchedDictionary<string, Decimal>((await this._orderActionsRepository.GetAsync(stocksCacheAsync.Id)).GroupBy<StockOrderAction, string>((Func<StockOrderAction, string>) (x => x.WarehouseId)).ToDictionary<IGrouping<string, StockOrderAction>, string, Decimal>((Func<IGrouping<string, StockOrderAction>, string>) (g => g.Key), (Func<IGrouping<string, StockOrderAction>, Decimal>) (g => g.Sum<StockOrderAction>((Func<StockOrderAction, Decimal>) (x => x.Quantity)))));
    AggregatedStockOrderLine line1 = line;
    line = (AggregatedStockOrderLine) null;
    return line1;
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
    this.SelectedLine = this.Details.Lines.RemoveWithSelection<AggregatedStockOrderLine>(this.SelectedLine);
  }

  public ICommand NewTransferCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnNewTransferCommandAsync), (Func<bool>) (() => !this.IsBusy && !this.IsDirty && this.Details.IsCompleted && !string.IsNullOrEmpty(this.Details.WarehouseId)));
    }
  }

  protected virtual async Task OnNewTransferCommandAsync()
  {
    AggregatedStockOrderDetailsViewModel detailsViewModel = this;
    IMvxNavigationService navigationService = detailsViewModel.NavigationService;
    NewStockTransferDialogViewModel.Params @params = new NewStockTransferDialogViewModel.Params();
    @params.Warehouses = detailsViewModel.Warehouses.List.Where<Warehouse>((Func<Warehouse, bool>) (x => x.Id != this.Details.WarehouseId));
    @params.SourceWarehouse = detailsViewModel.Warehouses.List.Single<Warehouse>((Func<Warehouse, bool>) (x => x.Id == this.Details.WarehouseId));
    CancellationToken cancellationToken = new CancellationToken();
    string destinationWarehouseId = await navigationService.Navigate<NewStockTransferDialogViewModel, NewStockTransferDialogViewModel.Params, string>(@params, cancellationToken: cancellationToken);
    if (string.IsNullOrEmpty(destinationWarehouseId))
      ;
    else
    {
      CopyCreateLine[] array = detailsViewModel.Details.Lines.SelectMany<AggregatedStockOrderLine, KeyValuePair<string, Decimal>, CopyCreateLine>((Func<AggregatedStockOrderLine, IEnumerable<KeyValuePair<string, Decimal>>>) (x => x.Orders.Where<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (i => i.Key == destinationWarehouseId && i.Value > 0M))), (Func<AggregatedStockOrderLine, KeyValuePair<string, Decimal>, CopyCreateLine>) ((x, order) => new CopyCreateLine()
      {
        StockId = x.StockId,
        Quantity = new Decimal?(order.Value),
        UnitId = x.UnitId
      })).ToArray<CopyCreateLine>();
      if (!((IEnumerable<CopyCreateLine>) array).Any<CopyCreateLine>())
        detailsViewModel.UserInteractionService.ShowMessage(detailsViewModel["Exception", Array.Empty<object>()], detailsViewModel["There are no orders for selected warehouse!", Array.Empty<object>()]);
      else
        await detailsViewModel.NavigationService.Navigate<StockTransferDetailsViewModel, StockTransferDetailsViewModel.Params>(new StockTransferDetailsViewModel.Params()
        {
          SourceWarehouseId = detailsViewModel.Details.WarehouseId,
          DestinationWarehouseId = destinationWarehouseId,
          Lines = (IEnumerable<CopyCreateLine>) array
        });
    }
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
    AggregatedStockOrderDetailsViewModel detailsViewModel = this;
    IEnumerable<object> source = await detailsViewModel.NavigationService.Navigate<DataImportViewModel, Type, IEnumerable<object>>(typeof (AggregatedStockOrderDetailsViewModel.LineImport));
    int i = 0;
    detailsViewModel.IsBusy = true;
    detailsViewModel.SuspendLoading = true;
    try
    {
      AggregatedStockOrderDetailsViewModel.LineImport[] array = source != null ? source.Cast<AggregatedStockOrderDetailsViewModel.LineImport>().ToArray<AggregatedStockOrderDetailsViewModel.LineImport>() : (AggregatedStockOrderDetailsViewModel.LineImport[]) null;
      if (array != null)
      {
        int itemsCount = array.Length;
        AggregatedStockOrderDetailsViewModel.LineImport[] lineImportArray = array;
        for (int index = 0; index < lineImportArray.Length; ++index)
        {
          AggregatedStockOrderDetailsViewModel.LineImport item = lineImportArray[index];
          ++i;
          detailsViewModel.Status = detailsViewModel["Importing {0} of {1} lines", new object[2]
          {
            (object) i,
            (object) itemsCount
          }];
          Stock stock = detailsViewModel.StocksCache.SingleOrDefault<Stock>((Func<Stock, bool>) (x => x.Code != item.StockCode));
          if (stock == null)
          {
            stock = (await detailsViewModel._stocksRepository.GetAsync((Expression<Func<Stock, bool>>) (x => x.Code == item.StockCode))).Single<Stock>();
            detailsViewModel.StocksCache.Add(stock);
          }
          AggregatedStockOrderLine aggregatedStockOrderLine = new AggregatedStockOrderLine()
          {
            StockId = stock.Id
          };
          detailsViewModel.Details.Lines.Add(aggregatedStockOrderLine);
        }
        lineImportArray = (AggregatedStockOrderDetailsViewModel.LineImport[]) null;
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

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
           
            if (string.IsNullOrEmpty(Details.WarehouseId))
            {
                throw new Exception(this["Field '{0}' is required", this["Warehouse"]]);
            }

            
            if (Details.Lines == null || !Details.Lines.Any())
            {
                throw new Exception(this["Document cannot be empty"]);
            }
        }
        catch (Exception ex)
        {
            
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        return await base.OnSaveAsync();
    }
    public class Params
  {
    public string WarehouseId { get; set; }

    public IEnumerable<string> StockIds { get; set; }
  }

  public class LineImport
  {
    public string StockCode { get; internal set; }
  }
}
