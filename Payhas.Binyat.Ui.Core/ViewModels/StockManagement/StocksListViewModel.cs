// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.StocksListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Data;
using Payhas.Data.Authorizers;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement;

public class StocksListViewModel : 
  ListViewModelBase<StockInfo>,
  IMvxViewModel<string, string>,
  IMvxViewModel<string>,
  IMvxViewModel,
  IMvxViewModelResult<string>
{
  protected string ItemId;
  private readonly ICouchCluster _couchCluster;
  private readonly IStocksRepository _repository;
  private readonly IListAuthorizer<Stock> _authorizer;
  private readonly IStockCodeGenerationService _codeGenerationService;
  private string _additionalPriceCurrencyId;
  private string _additionalPriceGroup;
  private string[] _priceGroupNames;

  public StocksListViewModel(
    IMvxMessenger messenger,
    ICouchCluster couchCluster,
    IStocksRepository repository,
    IListAuthorizer<Stock> authorizer,
    Reference<Currency> currencyReference,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService,
    IStockCodeGenerationService codeGenerationService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._couchCluster = couchCluster;
    this._repository = repository;
    this._authorizer = authorizer;
    this._codeGenerationService = codeGenerationService;
    this.Currencies = currencyReference;
  }

  public override string Caption => this["Stocks", Array.Empty<object>()];

  public virtual string AdditionalPriceCurrencyId
  {
    get => this._additionalPriceCurrencyId;
    set
    {
      if (!this.SetProperty<string>(ref this._additionalPriceCurrencyId, value, nameof (AdditionalPriceCurrencyId)) || this.IsBusy)
        return;
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.ShowAdditionalPrice));
      this.Initialize();
    }
  }

  public virtual string AdditionalPriceGroup
  {
    get => this._additionalPriceGroup;
    set
    {
      if (!this.SetProperty<string>(ref this._additionalPriceGroup, value, nameof (AdditionalPriceGroup)) || this.IsBusy)
        return;
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.ShowAdditionalPrice));
      this.Initialize();
    }
  }

  public virtual bool ShowAdditionalPrice
  {
    get
    {
      return !string.IsNullOrEmpty(this.AdditionalPriceGroup) || !string.IsNullOrEmpty(this.AdditionalPriceCurrencyId);
    }
  }

  public Reference<Currency> Currencies { get; }

  public bool HasCreateAccess => this._authorizer.CanCreate();

  public virtual string[] PriceGroupNames
  {
    get => this._priceGroupNames;
    set => this.SetProperty<string[]>(ref this._priceGroupNames, value, nameof (PriceGroupNames));
  }

  protected async Task LoadFacetsAsync()
  {
    this.PriceGroupNames = (await this._repository.GetFacets("PriceGroupNames"))["PriceGroupNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }

  public void Prepare(string parameter) => this.ItemId = parameter;

  protected override Task PreLoad()
  {
    return Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync(), this.Currencies.Initialize());
  }

  protected override async Task OnLoad()
  {
    StocksListViewModel stocksListViewModel = this;
    IEnumerable<StockInfo> infoAsync = await stocksListViewModel._repository.GetInfoAsync(stocksListViewModel.AdditionalPriceCurrencyId, stocksListViewModel.AdditionalPriceGroup);
    stocksListViewModel.List = infoAsync;
    if (string.IsNullOrEmpty(stocksListViewModel.ItemId))
      return;
    // ISSUE: reference to a compiler-generated method
    stocksListViewModel.SelectedItem = stocksListViewModel.List.SingleOrDefault<StockInfo>(new Func<StockInfo, bool>(stocksListViewModel.\u003COnLoad\u003Eb__30_0));
  }

  public ICommand CreateNewCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateNewAsync), (Func<bool>) (() => !this.IsBusy && this.HasCreateAccess));
    }
  }

  protected virtual Task OnCreateNewAsync()
  {
    return this.NavigationService.Navigate<DetailsViewModel<Stock>, string>(string.Empty);
  }

  public ICommand ViewDetailsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnViewDetailsAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedItem != null));
    }
  }

  protected virtual Task OnViewDetailsAsync()
  {
    return this.NavigationService.Navigate<DetailsViewModel<Stock>, string>(this.SelectedItem.Id);
  }

  public ICommand SelectOrViewDetailsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectOrViewDetailsAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedItem != null));
    }
  }

  protected virtual Task OnSelectOrViewDetailsAsync()
  {
    if (!string.IsNullOrEmpty(this.ItemId))
      return (Task) this.NavigationService.Close<string>((IMvxViewModelResult<string>) this, this.SelectedItem.Id);
    this.ViewDetailsCommand.Execute((object) null);
    return Task.CompletedTask;
  }

  public ICommand ImportCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnImportCommandAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnImportCommandAsync()
  {
    StocksListViewModel stocksListViewModel = this;
    IEnumerable<object> source1 = await stocksListViewModel.NavigationService.Navigate<DataImportViewModel, Type, IEnumerable<object>>(typeof (StocksListViewModel.StockImport));
    int i = 0;
    stocksListViewModel.IsBusy = true;
    stocksListViewModel.SuspendLoading = true;
    try
    {
      IEnumerable<StocksListViewModel.StockImport> source2 = source1 != null ? source1.Cast<StocksListViewModel.StockImport>() : (IEnumerable<StocksListViewModel.StockImport>) null;
      if (source2 != null)
      {
        if (!(source2 is StocksListViewModel.StockImport[] stockImportArray1))
          stockImportArray1 = source2.ToArray<StocksListViewModel.StockImport>();
        StocksListViewModel.StockImport[] stockImports = stockImportArray1;
        StocksListViewModel.StockImport[] stockImportArray = stockImports;
        for (int index = 0; index < stockImportArray.Length; ++index)
        {
          StocksListViewModel.StockImport item = stockImportArray[index];
          ++i;
          stocksListViewModel.Status = stocksListViewModel["Importing {0} of {1} items", new object[2]
          {
            (object) i,
            (object) stockImports.Length
          }];
          bool exists = true;
          Stock model = (Stock) null;
          if (!string.IsNullOrEmpty(item.Code))
            model = (await stocksListViewModel._repository.GetAsync((Expression<Func<Stock, bool>>) (x => x.Code == item.Code))).FirstOrDefault<Stock>();
          if (model == null)
          {
            exists = false;
            Stock stock1 = new Stock();
            stock1.Id = Guid.NewGuid().ToString();
            Stock stock2 = stock1;
            string str = item.Code;
            if (str == null)
              str = await stocksListViewModel._codeGenerationService.GetNextCode();
            stock2.Code = str;
            stock1.Units = new ObservableCollection<StockUnit>();
            stock1.Prices = new WatchedObservableCollection<StockPrice>();
            stock1.Barcodes = (IEnumerable<string>) new string[0];
            stock1.Tags = (IEnumerable<string>) new string[0];
            model = stock1;
            stock2 = (Stock) null;
            stock1 = (Stock) null;
          }
          if (!string.IsNullOrEmpty(item.Name))
            model.Name = item.Name;
          if (!string.IsNullOrEmpty(item.Unit) && model.Unit != item.Unit)
            model.Unit = item.Unit;
          if (item.Price > 0M && !string.IsNullOrEmpty(item.Currency))
          {
            Currency currency = stocksListViewModel.Currencies.List.Single<Currency>((Func<Currency, bool>) (x => x.Name == item.Currency));
            if (string.IsNullOrEmpty(item.PriceGroup))
            {
              if (model.Price != item.Price || model.CurrencyId != currency.Id)
              {
                model.Price = item.Price;
                model.CurrencyId = currency.Id;
              }
            }
            else
            {
              if (model.AdditionalPrices == null)
                model.AdditionalPrices = new WatchedObservableCollection<StockAdditionalPrice>();
              StockAdditionalPrice stockAdditionalPrice1 = model.AdditionalPrices.FirstOrDefault<StockAdditionalPrice>((Func<StockAdditionalPrice, bool>) (x => x.Group == item.PriceGroup && x.ValidFrom.Date == DateTime.Today.Date));
              if (stockAdditionalPrice1 == null)
              {
                StockAdditionalPrice stockAdditionalPrice2 = new StockAdditionalPrice();
                stockAdditionalPrice2.Group = item.PriceGroup;
                stockAdditionalPrice2.ValidFrom = DateTime.Today;
                stockAdditionalPrice1 = stockAdditionalPrice2;
                model.AdditionalPrices.Add(stockAdditionalPrice1);
              }
              stockAdditionalPrice1.Price = item.Price;
              stockAdditionalPrice1.CurrencyId = currency.Id;
            }
          }
          if (!string.IsNullOrEmpty(item.Group))
            model.Group = item.Group;
          if (!string.IsNullOrEmpty(item.Type))
            model.Type = item.Type;
          if (!string.IsNullOrEmpty(item.Barcodes))
            model.Barcodes = ((IEnumerable<string>) ((object) model.Barcodes ?? (object) new string[0])).Union<string>(((IEnumerable<string>) item.Barcodes.Split(',')).Select<string, string>((Func<string, string>) (x => x.Trim())).Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x)))).Distinct<string>();
          if (!string.IsNullOrEmpty(item.Tags))
            model.Tags = ((IEnumerable<string>) ((object) model.Tags ?? (object) new string[0])).Union<string>(((IEnumerable<string>) item.Tags.Split(',')).Select<string, string>((Func<string, string>) (x => x.Trim())).Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x)))).Distinct<string>();
          if (item.LimitMin > 0M)
            model.LimitMin = new Decimal?(item.LimitMin);
          if (item.LimitMax > 0M)
            model.LimitMax = new Decimal?(item.LimitMax);
          if (exists)
            await stocksListViewModel._repository.UpdateAsync(model);
          else
            await stocksListViewModel._repository.CreateAsync(model);
        }
        stockImportArray = (StocksListViewModel.StockImport[]) null;
        stockImports = (StocksListViewModel.StockImport[]) null;
      }
    }
    catch (Exception ex)
    {
      stocksListViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    stocksListViewModel.Status = (string) null;
    stocksListViewModel.SuspendLoading = false;
    stocksListViewModel.IsBusy = false;
    stocksListViewModel.ReloadCommand.Execute((object) null);
  }

  public ICommand MergeCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnMergeAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  public virtual Task OnMergeAsync()
  {
    return this.NavigationService.Navigate<StockMergerDialogViewModel>();
  }

  public ICommand FixSynchIssuesCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnFixSynchIssuesAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnFixSynchIssuesAsync()
  {
    StocksListViewModel stocksListViewModel = this;
    stocksListViewModel.IsBusy = true;
    try
    {
      using (IBucket bucket = stocksListViewModel._couchCluster.OpenDefaultBucket())
      {
        IQueryable<Stock> source = new BucketContext(bucket).Query<Stock>();
        Expression<Func<Stock, bool>> predicate = (Expression<Func<Stock, bool>>) (x => x.DocType == "Stock" && x.Id == N1QlFunctions.Key(x) && x.Prices.Any<StockPrice>((Func<StockPrice, bool>) (p => p.CurrencyId == default (string))));
        foreach (Stock stock in await source.Where<Stock>(predicate).ExecuteAsync<Stock>())
        {
          stock.Prices = new WatchedObservableCollection<StockPrice>(stock.Prices.Where<StockPrice>((Func<StockPrice, bool>) (x => !string.IsNullOrEmpty(x.CurrencyId))));
          IDocumentResult<Stock> documentResult = await bucket.ReplaceAsync<Stock>((IDocument<Stock>) new Document<Stock>()
          {
            Id = stock.Id,
            Content = stock
          });
        }
      }
    }
    catch (Exception ex)
    {
      stocksListViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    stocksListViewModel.IsBusy = false;
  }

  public class StockImport
  {
    public string Code { get; set; }

    public string Name { get; set; }

    public string Unit { get; set; }

    public Decimal Price { get; set; }

    public string Currency { get; set; }

    public string PriceGroup { get; set; }

    public string Barcodes { get; set; }

    public string Group { get; set; }

    public string Type { get; set; }

    public string Tags { get; set; }

    public Decimal LimitMin { get; set; }

    public Decimal LimitMax { get; set; }
  }
}
