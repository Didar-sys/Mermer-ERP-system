// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.SelectStockAlternativeViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement;

public class SelectStockAlternativeViewModel : 
  DialogViewModel,
  IMvxViewModel<Tuple<string, string>, string>,
  IMvxViewModel<Tuple<string, string>>,
  IMvxViewModel,
  IMvxViewModelResult<string>
{
  private readonly IStocksRepository _stocksRepository;
  private readonly IStockAlternativesRepository _repository;
  private readonly IStockBalancesRepository _balancesRepository;
  private IEnumerable<StockSearchResult> _list;
  private StockSearchResult _selectedItem;
  private string _stockId;
  private string _warehouseId;

  public SelectStockAlternativeViewModel(
    IMvxMessenger messenger,
    Reference<Currency> currencies,
    IStocksRepository stocksRepository,
    IStockAlternativesRepository repository,
    IMvxNavigationService navigationService,
    IStockBalancesRepository balancesRepository,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._repository = repository;
    this._stocksRepository = stocksRepository;
    this._balancesRepository = balancesRepository;
    this.Currencies = currencies;
  }

  public Reference<Currency> Currencies { get; }

  public IEnumerable<StockSearchResult> List
  {
    get => this._list;
    set => this.SetProperty<IEnumerable<StockSearchResult>>(ref this._list, value, nameof (List));
  }

  public StockSearchResult SelectedItem
  {
    get => this._selectedItem;
    set
    {
      this.SetProperty<StockSearchResult>(ref this._selectedItem, value, nameof (SelectedItem));
    }
  }

  public void Prepare(Tuple<string, string> parameter)
  {
    this._stockId = parameter.Item1;
    this._warehouseId = parameter.Item2;
  }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Currencies.Initialize());

  protected override async Task OnLoad()
  {
    await base.OnLoad();
    SingleStockAlternative alternativesAsync = await this._repository.GetAlternativesAsync(this._stockId);
    bool? nullable;
    if (alternativesAsync == null)
    {
      nullable = new bool?();
    }
    else
    {
      IEnumerable<string> alternatives = alternativesAsync.Alternatives;
      nullable = alternatives != null ? new bool?(alternatives.Any<string>()) : new bool?();
    }
    string[] stockIds;
    StockSearchResult[] result;
    if (!nullable.GetValueOrDefault())
    {
      stockIds = (string[]) null;
      result = (StockSearchResult[]) null;
    }
    else
    {
      stockIds = alternativesAsync.Alternatives.ToArray<string>();
      result = (await this._stocksRepository.GetListAsync(stockIds)).Select<Stock, StockSearchResult>((Func<Stock, StockSearchResult>) (x => new StockSearchResult()
      {
        Id = x.Id,
        Code = x.Code,
        Name = x.Name,
        Price = x.Price,
        CurrencyId = x.CurrencyId,
        Unit = x.Unit,
        IsDisabled = x.IsDisabled
      })).ToArray<StockSearchResult>();
      StockBalance[] array = (await this._balancesRepository.GetAsync(this._warehouseId, stockIds)).ToArray<StockBalance>();
      foreach (StockSearchResult stockSearchResult in result)
      {
        StockSearchResult item = stockSearchResult;
        item.Balance = ((IEnumerable<StockBalance>) array).Where<StockBalance>((Func<StockBalance, bool>) (x => x.StockId == item.Id)).Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Balance));
      }
      this.List = (IEnumerable<StockSearchResult>) result;
      stockIds = (string[]) null;
      result = (StockSearchResult[]) null;
    }
  }

  public ICommand SelectCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectCommandAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual Task OnSelectCommandAsync()
  {
    return (Task) this.NavigationService.Close<string>((IMvxViewModelResult<string>) this, this.SelectedItem?.Id);
  }

  public override Task<bool> OnCloseAsync()
  {
    return this.NavigationService.Close<string>((IMvxViewModelResult<string>) this, (string) null);
  }
}
