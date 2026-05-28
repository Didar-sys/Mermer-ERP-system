// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockRepriceDialogViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.StockManagement.Models;
using Mermer.Transactions.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Data.Storage;
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

public class StockRepriceDialogViewModel : 
  DialogViewModel,
  IMvxViewModel<IEnumerable<StockRepriceRequest>>,
  IMvxViewModel
{
  private StockRepriceRequest[] _requests;
  private Dictionary<string, CurrencyConvertion> _currencyConvertions;
  private readonly IRepository<Stock> _stocksRepository;
  private int _repricePercentage;
  private ObservableCollection<StockReprice> _list;
  private ObservableCollection<StockReprice> _selectedItems;

  public StockRepriceDialogViewModel(
    IMvxMessenger messenger,
    Reference<Currency> currencies,
    IRepository<Stock> stocksRepository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._stocksRepository = stocksRepository;
    this.Currencies = currencies;
  }

  public Reference<Currency> Currencies { get; }

  public virtual int RepricePercentage
  {
    get => this._repricePercentage;
    set => this.SetProperty<int>(ref this._repricePercentage, value, nameof (RepricePercentage));
  }

  public virtual ObservableCollection<StockReprice> List
  {
    get => this._list;
    set
    {
      if (this._list != null)
        this._list.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.List_CollectionChanged);
      this.SetProperty<ObservableCollection<StockReprice>>(ref this._list, value, nameof (List));
      if (this._list != null)
        this._list.CollectionChanged += new NotifyCollectionChangedEventHandler(this.List_CollectionChanged);
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItems));
    }
  }

  public bool HasAnyItems
  {
    get
    {
      ObservableCollection<StockReprice> list = this.List;
      return list != null && list.Any<StockReprice>();
    }
  }

  public ObservableCollection<StockReprice> SelectedItems
  {
    get => this._selectedItems;
    set
    {
      if (this._selectedItems != null)
        this._selectedItems.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.SelectedItems_CollectionChanged);
      this.SetProperty<ObservableCollection<StockReprice>>(ref this._selectedItems, value, nameof (SelectedItems));
      if (this._selectedItems != null)
        this._selectedItems.CollectionChanged += new NotifyCollectionChangedEventHandler(this.SelectedItems_CollectionChanged);
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItemsSelected));
    }
  }

  public bool HasAnyItemsSelected
  {
    get
    {
      ObservableCollection<StockReprice> selectedItems = this.SelectedItems;
      return selectedItems != null && selectedItems.Any<StockReprice>();
    }
  }

  private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItems));
  }

  private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItemsSelected));
  }

  public void Prepare(IEnumerable<StockRepriceRequest> parameter)
  {
    if (!(parameter is StockRepriceRequest[] stockRepriceRequestArray))
      stockRepriceRequestArray = parameter.ToArray<StockRepriceRequest>();
    this._requests = stockRepriceRequestArray;
  }

  protected override async Task PreLoad()
  {
    await Task.WhenAll(base.PreLoad(), this.Currencies.Initialize());
    this._currencyConvertions = this.Currencies.List.ToDictionary<Currency, string, CurrencyConvertion>((Func<Currency, string>) (x => x.Id), (Func<Currency, CurrencyConvertion>) (x =>
    {
      CurrencyRate rate = x.GetRate();
      return new CurrencyConvertion()
      {
        CurrencyId = x.Id,
        Multiplier = rate.Multiplier,
        Divider = rate.Divider
      };
    }));
  }

  protected override async Task OnLoad()
{
    SelectedItems = new ObservableCollection<StockReprice>();
    var list = new List<StockReprice>();
    
    for (int i = 0; i < _requests.Length; i += 100)
    {
        var requestsBatch = _requests.Skip(i).Take(100).ToList();
        var stocksToLoad = requestsBatch.Select(x => x.StockId).ToList();
        
        var expressionArray = new Expression<Func<Stock, bool>>[]
        {
            x => stocksToLoad.Contains(x.Id)
        };
        
        var loadedStocks = await _stocksRepository.GetAsync(expressionArray);

            // Відновлений людський LINQ
            var batchReprices = loadedStocks
                .Join(requestsBatch,
                      s => s.Id,
                      r => r.StockId,
                      (s, r) =>
                      {
                          // 1. Присваиваем Stock. 
                          // В этот момент сеттер класса StockReprice 
                          // автоматически запишет текущую (старую) s.Price в CurrentPrice.
                          var stockReprice = new StockReprice
                          {
                              Stock = s
                          };

                          // 2. Обновляем цену на новую из запроса.
                          // Это вызовет Stock_PropertyChanged внутри StockReprice 
                          // и обновит свойство NewPriceValue для UI.
                          stockReprice.Stock.Price = r.ReferencePrice;

                          return stockReprice;
                      });

            list.AddRange(batchReprices);
    }
    
    List = new ObservableCollection<StockReprice>(list);
}

  private CurrencyConvertion CurrencyConverter(string currencyId)
  {
    return this._currencyConvertions[currencyId];
  }

  public ICommand CalculateNewPriceCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnCalculateNewPriceCommand), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private void OnCalculateNewPriceCommand()
  {
    foreach (var stockReprice in List)
    {
        stockReprice.Stock.Price = stockReprice.ReferencePrice * (0.01M * (Decimal) this.RepricePercentage + 1M);
        stockReprice.Stock.CurrencyId = stockReprice.ReferencePriceCurrencyId;
    }
  }

  public ICommand SelectedItemsDeleteCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnSelectedItemsDeleteCommand), (Func<bool>) (() => !this.IsBusy && this.HasAnyItemsSelected));
    }
  }

  protected virtual void OnSelectedItemsDeleteCommand()
  {
    this.IsBusy = true;
    try
    {
      StockReprice[] array = this.SelectedItems.ToArray<StockReprice>();
      this.SelectedItems = new ObservableCollection<StockReprice>();
      foreach (StockReprice stockReprice in array)
        this.List.Remove(stockReprice);
    }
    catch (Exception ex)
    {
      this.UserInteractionService.ShowExceptionMessage(ex);
    }
    this.IsBusy = false;
  }

  public ICommand RepriceCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnRepriceCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasAnyItems));
    }
  }

  protected virtual async Task OnRepriceCommandAsync()
  {
    StockRepriceDialogViewModel repriceDialogViewModel = this;
    repriceDialogViewModel.IsBusy = true;
    try
    {
      foreach (StockReprice stockReprice in (Collection<StockReprice>) repriceDialogViewModel.List)
        await repriceDialogViewModel._stocksRepository.UpdateAsync(stockReprice.Stock);
      int num = await repriceDialogViewModel.OnCloseAsync() ? 1 : 0;
    }
    catch (Exception ex)
    {
      repriceDialogViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    repriceDialogViewModel.IsBusy = false;
  }
}
