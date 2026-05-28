// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.Ordering.StockOrderDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Authorization.Services;
using Mermer.Common.Settings;
using Mermer.Enterprise.Models;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Transactions.Models;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.Services;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Data.Tools;
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
namespace Mermer.Ui.Core.ViewModels.Warehousing.Ordering;

public class StockOrderDetailsViewModel : 
  TransactionDetailsViewModel<StockOrder>,
  IMvxViewModel<IEnumerable<CopyCreateLine>>,
  IMvxViewModel
{
  private readonly IConfigurator _configurator;
  private readonly IPrintingService _printingService;
  private readonly IRepository<Stock> _stocksRepository;
  private ObservableCollection<Stock> _stocksCache;
  private string[] _groupNames;
  private string[] _tagNames;
  private IEnumerable<CopyCreateLine> _stockLineCopies;
  private Decimal _addQuantity = 1M;
  private StockOrderLine _selectedLine;

  public StockOrderDetailsViewModel(
    CopyCreate copyCreate,
    IConfigurator configurator,
    ILoginService loginService,
    StockSearcher stockSearcher,
    IPrintingService printingService,
    Reference<Warehouse> warehouses,
    IRepository<StockOrder> repository,
    IListAuthorizer<StockOrder> authorizer,
    IRepository<Stock> stocksRepository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService,
    ITransactionCodeGenerationService codeGenerationService)
    : base(codeGenerationService, repository, authorizer, loginService, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._printingService = printingService;
    this._stocksRepository = stocksRepository;
    this.CopyCreate = copyCreate;
    this.CopyCreate.GetLines = (Func<IEnumerable<CopyCreateLine>>) (() => this.Details.Lines.Select<StockOrderLine, CopyCreateLine>((Func<StockOrderLine, CopyCreateLine>) (x => new CopyCreateLine()
    {
      StockId = x.StockId,
      Quantity = new Decimal?(x.Quantity),
      UnitId = x.UnitId
    })));
    this.StockSearcher = stockSearcher;
    this.StockSearcher.ResultSelected += new SearchResultSelected(this.StockSearcher_ResultSelected);
    this.Warehouses = warehouses;
  }

  public ObservableCollection<Stock> StocksCache
  {
    get => this._stocksCache;
    set
    {
      this.SetProperty<ObservableCollection<Stock>>(ref this._stocksCache, value, nameof (StocksCache));
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

  protected virtual async Task LoadFacetsAsync()
  {
    StockOrderDetailsViewModel detailsViewModel = this;
    Dictionary<string, Dictionary<string, int>> facets = await ((IRepositoryWithFacets<StockOrder>) detailsViewModel.Repository).GetFacets("GroupNames", "TagNames");
    detailsViewModel.GroupNames = facets["GroupNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
    detailsViewModel.TagNames = facets["TagNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }

  public void Prepare(IEnumerable<CopyCreateLine> parameter) => this._stockLineCopies = parameter;

  protected override Task PreLoad()
  {
    this.StocksCache = new ObservableCollection<Stock>();
    return Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync(), this.Warehouses.Initialize(), this.StockSearcher.Initialize());
  }

    protected override async Task PostLoad()
    {
        await base.PostLoad();

        if (string.IsNullOrEmpty(ItemId))
        {
            AppSettings configAsync = await _configurator.GetConfigAsync<AppSettings>();
            Details.WarehouseId = configAsync.DefaultWarehouseId;
        }

        if (Details.Lines == null)
        {
            Details.Lines = new ObservableCollection<StockOrderLine>();
            if (_stockLineCopies != null)
            {
                foreach (CopyCreateLine stockLineCopy in _stockLineCopies)
                {
                    Details.Lines.Add(new StockOrderLine
                    {
                        StockId = stockLineCopy.StockId,
                        Quantity = stockLineCopy.Quantity.GetValueOrDefault(),
                        UnitId = stockLineCopy.UnitId
                    });
                }
            }
        }

        var watcher = new ObservableCollectionWatcher<StockOrderLine>(Details.Lines);
        watcher.ItemPropertyChanged += Line_PropertyChanged;

        if (Details.StockUnitConvertions == null)
        {
            Details.StockUnitConvertions = new ObservableCollection<StockUnitConvertion>();
        }

        StockSearcher.WarehouseId = Details.WarehouseId;
        Details.PropertyChanged += Details_PropertyChanged;

        await LoadStocksCache();

        // Відновлена лямбда фільтрації
        Warehouses.Filter = w => !w.IsDisabled || w.Id == Details.WarehouseId;
    }

    private void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!(e.PropertyName == "WarehouseId"))
      return;
    this.StockSearcher.WarehouseId = this.Details.WarehouseId;
  }

    private void Line_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "UnitId")
            return;

        // Виправлений синтаксис перевірки типу
        if (sender is StockOrderLine stockOrderLine)
        {
            UpdateStockUnitConvertion(stockOrderLine.StockId, stockOrderLine.UnitId);
        }
    }

    private async Task LoadStocksCache()
  {
    StockOrderDetailsViewModel detailsViewModel = this;
    ObservableCollection<Stock> cache = new ObservableCollection<Stock>();
    foreach (StockOrderLine line1 in (Collection<StockOrderLine>) detailsViewModel.Details.Lines)
    {
      StockOrderLine line = line1;
      if (!cache.Any<Stock>((Func<Stock, bool>) (x => x.Id == line.StockId)))
        cache.Add(await detailsViewModel._stocksRepository.GetAsync(line.StockId));
    }
    detailsViewModel.StocksCache = cache;
    if (detailsViewModel._stockLineCopies == null)
    {
      cache = (ObservableCollection<Stock>) null;
    }
    else
    {
      foreach (CopyCreateLine copyCreateLine in detailsViewModel._stockLineCopies.Where<CopyCreateLine>((Func<CopyCreateLine, bool>) (x => !string.IsNullOrEmpty(x.UnitId))))
        detailsViewModel.UpdateStockUnitConvertion(copyCreateLine.StockId, copyCreateLine.UnitId);
      detailsViewModel._stockLineCopies = (IEnumerable<CopyCreateLine>) null;
      cache = (ObservableCollection<Stock>) null;
    }
  }

  private async void StockSearcher_ResultSelected(StockSearcher searcher, StockSearchResult result)
  {
    StockOrderDetailsViewModel detailsViewModel = this;
    if (detailsViewModel.StocksCache.All<Stock>((Func<Stock, bool>) (x => x.Id != result.Id)))
    {
      Stock async = await detailsViewModel._stocksRepository.GetAsync(result.Id);
      detailsViewModel.StocksCache.Add(async);
    }
    StockOrderLine stockOrderLine = new StockOrderLine()
    {
      StockId = result.Id,
      Quantity = detailsViewModel.AddQuantity,
      UnitId = result.UnitId
    };
    detailsViewModel.UpdateStockUnitConvertion(stockOrderLine.StockId, stockOrderLine.UnitId);
    detailsViewModel.Details.Lines.Add(stockOrderLine);
    detailsViewModel.AddQuantity = 1M;
    detailsViewModel.SelectedLine = stockOrderLine;
    detailsViewModel.SelectedLineEditCommand.Execute((object) null);
  }

  private void UpdateStockUnitConvertion(string stockId, string unitId)
  {
    if (this.Details.StockUnitConvertions.Any<StockUnitConvertion>((Func<StockUnitConvertion, bool>) (x => x.StockId == stockId && x.UnitId == unitId)))
      return;
    StockUnit stockUnit = this.StocksCache.Single<Stock>((Func<Stock, bool>) (x => x.Id == stockId)).Units.Single<StockUnit>((Func<StockUnit, bool>) (x => x.Id == unitId));
    this.Details.StockUnitConvertions.Add(new StockUnitConvertion()
    {
      StockId = stockId,
      UnitId = stockUnit.Id,
      Multiplier = stockUnit.Multiplier,
      Divider = stockUnit.Divider
    });
  }

    protected override async Task<bool> OnSaveAsync()
    {
        if (!await base.OnSaveAsync())
            return false;

        await _printingService.PrintStockOrder(Details);
        return true;
    }

    public CopyCreate CopyCreate { get; }

  public StockSearcher StockSearcher { get; set; }

  public Reference<Warehouse> Warehouses { get; set; }

  public virtual Decimal AddQuantity
  {
    get => this._addQuantity;
    set => this.SetProperty<Decimal>(ref this._addQuantity, value, nameof (AddQuantity));
  }

  public bool IsLineSelected => this.SelectedLine != null;

  public virtual StockOrderLine SelectedLine
  {
    get => this._selectedLine;
    set
    {
      this.SetProperty<StockOrderLine>(ref this._selectedLine, value, nameof (SelectedLine));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsLineSelected));
    }
  }

  public ICommand SelectedLineMinusOneCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.SelectedLineMinusOne), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

  private void SelectedLineMinusOne() => this.SelectedLine.Quantity -= 1M;

  public ICommand SelectedLinePlusOneCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.SelectedLinePlusOne), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

  private void SelectedLinePlusOne() => this.SelectedLine.Quantity += 1M;

  public ICommand SelectedLineDeleteCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.SelectedLineDelete), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

  private void SelectedLineDelete()
  {
    int num = this.Details.Lines.IndexOf(this.SelectedLine);
    this.Details.Lines.Remove(this.SelectedLine);
    int index = num - 1;
    if (index < 0)
      index = 0;
    if (index >= this.Details.Lines.Count)
      return;
    this.SelectedLine = this.Details.Lines.ElementAt<StockOrderLine>(index);
  }

  public ICommand SelectedLineEditCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectedLineEditAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

    protected virtual async Task OnSelectedLineEditAsync()
    {
        // Відновлена лямбда пошуку стоку
        Stock stock = StocksCache.Single(x => x.Id == SelectedLine.StockId);

        var parameters = new StockOrderDetailsLineEditViewModel.Params
        {
            StockCode = stock.Code,
            StockName = stock.Name,
            Quantity = SelectedLine.Quantity,
            UnitId = SelectedLine.UnitId,
            Units = stock.Units
        };

        var result = await NavigationService.Navigate<StockOrderDetailsLineEditViewModel, StockOrderDetailsLineEditViewModel.Params, StockOrderDetailsLineEditViewModel.Result>(parameters);

        if (result == null)
            return;

        SelectedLine.Quantity = result.Quantity;
        SelectedLine.UnitId = result.UnitId;
    }

    public ICommand SelectWarehouseCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.SelectWarehouseAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task SelectWarehouseAsync()
  {
    StockOrderDetailsViewModel detailsViewModel = this;
    StockOrder stockOrder = detailsViewModel.Details;
    stockOrder.WarehouseId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Warehouse>, string, string>(detailsViewModel.Details.WarehouseId);
    stockOrder = (StockOrder) null;
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
    StockOrderDetailsViewModel detailsViewModel = this;
    await detailsViewModel._printingService.PrintStockOrder(detailsViewModel.Details, true);
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
    StockOrderDetailsViewModel detailsViewModel = this;
    IEnumerable<object> source1 = await detailsViewModel.NavigationService.Navigate<DataImportViewModel, Type, IEnumerable<object>>(typeof (StockOrderDetailsViewModel.LineImport));
    int i = 0;
    detailsViewModel.IsBusy = true;
    detailsViewModel.SuspendLoading = true;
    try
    {
      IEnumerable<StockOrderDetailsViewModel.LineImport> source2 = source1 != null ? source1.Cast<StockOrderDetailsViewModel.LineImport>() : (IEnumerable<StockOrderDetailsViewModel.LineImport>) null;
      if (source2 != null)
      {
        int itemsCount = source2.Count<StockOrderDetailsViewModel.LineImport>();
        foreach (StockOrderDetailsViewModel.LineImport lineImport in source2)
        {
          StockOrderDetailsViewModel.LineImport item = lineImport;
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
          StockOrderLine stockOrderLine = new StockOrderLine()
          {
            StockId = stock.Id,
            Quantity = item.Quantity,
            UnitId = stock.Units.SingleOrDefault<StockUnit>((Func<StockUnit, bool>) (x => x.Name == item.Unit))?.Id
          };
          detailsViewModel.UpdateStockUnitConvertion(stockOrderLine.StockId, stockOrderLine.UnitId);
          detailsViewModel.Details.Lines.Add(stockOrderLine);
        }
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
    public string StockCode { get; internal set; }

    public Decimal Quantity { get; internal set; }

    public string Unit { get; internal set; }
  }
}
