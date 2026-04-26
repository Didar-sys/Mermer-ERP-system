// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.StockBalancesByStatusesListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using Humanizer;
using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Data.Tools.Expressions;
using Payhas.Mvvm.Messages;
using Payhas.Mvvm.Services;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement;

public class StockBalancesByStatusesListViewModel : ListViewModelBaseWithFilter<StockBalanceWithData>
{
  private readonly IConfigurator _configurator;
  private readonly IStocksRepository _stocksRepository;
  private readonly IStockBalancesRepository _balancesRepository;
  private readonly MvxSubscriptionToken _messageToken;
  private string _caption;
  private System.Collections.Generic.List<object> _selectedWarehouseIds;
  private string _displayCurrencyId;
  private bool _loaded;
  private IEnumerable<StockBalanceWithData> _balances;

  public StockBalancesByStatusesListViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    Reference<Currency> currencies,
    Reference<Warehouse> warehouses,
    IStocksRepository stocksRepository,
    IStockBalancesRepository balancesRepository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._stocksRepository = stocksRepository;
    this._balancesRepository = balancesRepository;
    this._messageToken = messenger.Subscribe<DocumentModified<StockBalance>>((Action<DocumentModified<StockBalance>>) (async m => await this.Initialize()), MvxReference.Strong);
    this.Currencies = currencies;
    this.Warehouses = warehouses;
    this.Filters = (IEnumerable<ListFilter>) new ListFilter[5]
    {
      new ListFilter()
      {
        Title = this["Existing", Array.Empty<object>()],
        Tag = (object) "Existing",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<StockBalanceWithData, StockBalanceWithData>) this).CountByFilterAsync)
      },
      new ListFilter()
      {
        Title = this["Finished", Array.Empty<object>()],
        Tag = (object) "Finished",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<StockBalanceWithData, StockBalanceWithData>) this).CountByFilterAsync)
      },
      new ListFilter()
      {
        Title = this["Small Amount", Array.Empty<object>()],
        Tag = (object) "Min",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<StockBalanceWithData, StockBalanceWithData>) this).CountByFilterAsync)
      },
      new ListFilter()
      {
        Title = this["Over Limit", Array.Empty<object>()],
        Tag = (object) "Max",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<StockBalanceWithData, StockBalanceWithData>) this).CountByFilterAsync)
      },
      new ListFilter()
      {
        Title = this["All Records", Array.Empty<object>()],
        Tag = (object) "All",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<StockBalanceWithData, StockBalanceWithData>) this).CountByFilterAsync)
      }
    };
  }

  public override string Caption
  {
    get => this._caption ?? this["StockBalance".Pluralize(), Array.Empty<object>()];
    set => this._caption = value;
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

  public virtual string DisplayCurrencyId
  {
    get => this._displayCurrencyId;
    set
    {
      if (!this.SetProperty<string>(ref this._displayCurrencyId, value, nameof (DisplayCurrencyId)) || this.IsBusy)
        return;
      this.Initialize();
    }
  }

  public Reference<Currency> Currencies { get; }

  public Reference<Warehouse> Warehouses { get; }

  protected override async Task PreLoad()
  {
    StockBalancesByStatusesListViewModel statusesListViewModel = this;
    await Task.WhenAll(statusesListViewModel.Currencies.Initialize(), statusesListViewModel.Warehouses.Initialize());
    if (!statusesListViewModel._loaded)
    {
      AppSettings configAsync = await statusesListViewModel._configurator.GetConfigAsync<AppSettings>();
      statusesListViewModel.SelectedWarehouseIds = new System.Collections.Generic.List<object>((IEnumerable<object>) new object[1]
      {
        (object) configAsync.DefaultWarehouseId
      });
      statusesListViewModel.DisplayCurrencyId = statusesListViewModel.Currencies.List.Single<Currency>((Func<Currency, bool>) (x => x.IsDefault)).Id;
    }
    statusesListViewModel._loaded = true;
    await statusesListViewModel.PreLoadBalances();
    // ISSUE: reference to a compiler-generated method
    await statusesListViewModel.\u003C\u003En__0();
  }

  private async Task PreLoadBalances()
  {
    Stock[] stocks;
    if (this.WarehouseIds.Length == 0)
    {
      stocks = (Stock[]) null;
    }
    else
    {
      stocks = (await this._stocksRepository.GetAsync()).ToArray<Stock>();
      IEnumerable<\u003C\u003Ef__AnonymousType1<string, Decimal, Decimal, Decimal>> inner = (await this._balancesRepository.GetAsync((string) null, DateTime.Now, this.WarehouseIds)).GroupBy<StockBalance, string>((Func<StockBalance, string>) (x => x.StockId)).Select(g => new
      {
        StockId = g.Key,
        Income = g.Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Income)),
        Expense = g.Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Expense)),
        Balance = g.Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Balance))
      });
      Currency currency = this.Currencies.List.Single<Currency>((Func<Currency, bool>) (x => x.Id == this.DisplayCurrencyId));
      CurrencyRate displayCurrencyRate = currency.GetRate();
      int displayCurrencyDecimals = currency.Decimals;
      this._balances = ((IEnumerable<Stock>) stocks).Join(this.Currencies.List, (Func<Stock, string>) (s => s.CurrencyId), (Func<Currency, string>) (c => c.Id), (s, c) => new
      {
        s = s,
        c = c
      }).GroupJoin(inner, _param1 => _param1.s.Id, sb => sb.StockId, (_param1, gj) => new
      {
        \u003C\u003Eh__TransparentIdentifier0 = _param1,
        gj = gj
      }).SelectMany(_param1 => _param1.gj.DefaultIfEmpty(), (_param1, sb) => new
      {
        \u003C\u003Eh__TransparentIdentifier1 = _param1,
        sb = sb
      }).Select(_param1 => new
      {
        \u003C\u003Eh__TransparentIdentifier2 = _param1,
        currencyRate = _param1.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.c.GetRate()
      }).Select(_param1 =>
      {
        StockBalanceWithData stockBalanceWithData = new StockBalanceWithData();
        stockBalanceWithData.StockId = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.Id;
        stockBalanceWithData.StockCode = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.Code;
        stockBalanceWithData.StockName = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.Name;
        stockBalanceWithData.StockUnit = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.Unit;
        stockBalanceWithData.StockPrice = Math.Round(_param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.Price * _param1.currencyRate.Multiplier / _param1.currencyRate.Divider / displayCurrencyRate.Multiplier * displayCurrencyRate.Divider, displayCurrencyDecimals);
        stockBalanceWithData.StockGroup = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.Group;
        stockBalanceWithData.StockType = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.Type;
        stockBalanceWithData.StockTags = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.Tags;
        var sb1 = _param1.\u003C\u003Eh__TransparentIdentifier2.sb;
        stockBalanceWithData.Income = sb1 != null ? sb1.Income : 0M;
        var sb2 = _param1.\u003C\u003Eh__TransparentIdentifier2.sb;
        stockBalanceWithData.Expense = sb2 != null ? sb2.Expense : 0M;
        var sb3 = _param1.\u003C\u003Eh__TransparentIdentifier2.sb;
        stockBalanceWithData.IsExisting = (sb3 != null ? sb3.Balance : 0M) > 0M;
        var sb4 = _param1.\u003C\u003Eh__TransparentIdentifier2.sb;
        stockBalanceWithData.IsFinished = (sb4 != null ? sb4.Balance : 0M) <= 0M;
        var sb5 = _param1.\u003C\u003Eh__TransparentIdentifier2.sb;
        stockBalanceWithData.IsOverUsed = (sb5 != null ? sb5.Balance : 0M) < 0M;
        Decimal? nullable = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.LimitMin;
        int num1;
        if (nullable.HasValue)
        {
          var sb6 = _param1.\u003C\u003Eh__TransparentIdentifier2.sb;
          Decimal balance = sb6 != null ? sb6.Balance : 0M;
          nullable = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.LimitMin;
          Decimal valueOrDefault = nullable.GetValueOrDefault();
          num1 = balance < valueOrDefault & nullable.HasValue ? 1 : 0;
        }
        else
          num1 = 0;
        stockBalanceWithData.IsFinishing = num1 != 0;
        nullable = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.LimitMax;
        int num2;
        if (nullable.HasValue)
        {
          var sb7 = _param1.\u003C\u003Eh__TransparentIdentifier2.sb;
          Decimal balance = sb7 != null ? sb7.Balance : 0M;
          nullable = _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.s.LimitMax;
          Decimal valueOrDefault = nullable.GetValueOrDefault();
          num2 = balance > valueOrDefault & nullable.HasValue ? 1 : 0;
        }
        else
          num2 = 0;
        stockBalanceWithData.IsOverLimit = num2 != 0;
        return stockBalanceWithData;
      });
      stocks = (Stock[]) null;
    }
  }

  protected override PredicateBuilder<StockBalanceWithData> GetPredicateBuilder(ListFilter filter)
  {
    PredicateBuilder<StockBalanceWithData> predicateBuilder = base.GetPredicateBuilder(filter);
    if (filter.Tag is string tag)
    {
      switch (tag)
      {
        case "Existing":
          predicateBuilder.Add((Expression<Func<StockBalanceWithData, bool>>) (x => x.IsExisting));
          break;
        case "Finished":
          predicateBuilder.Add((Expression<Func<StockBalanceWithData, bool>>) (x => x.IsFinished));
          break;
        case "Min":
          predicateBuilder.Add((Expression<Func<StockBalanceWithData, bool>>) (x => x.IsFinishing));
          break;
        case "Max":
          predicateBuilder.Add((Expression<Func<StockBalanceWithData, bool>>) (x => x.IsOverLimit));
          break;
      }
    }
    return predicateBuilder;
  }

  protected override Task<int> CountListAsync(
    params Expression<Func<StockBalanceWithData, bool>>[] predicates)
  {
    return Task.FromResult<int>(((IEnumerable<Expression<Func<StockBalanceWithData, bool>>>) predicates).Aggregate<Expression<Func<StockBalanceWithData, bool>>, IEnumerable<StockBalanceWithData>>(this._balances, (Func<IEnumerable<StockBalanceWithData>, Expression<Func<StockBalanceWithData, bool>>, IEnumerable<StockBalanceWithData>>) ((current, predicate) => current.Where<StockBalanceWithData>(predicate.Compile()))).Count<StockBalanceWithData>());
  }

  protected override Task<IEnumerable<StockBalanceWithData>> GetListAsync(
    params Expression<Func<StockBalanceWithData, bool>>[] predicates)
  {
    return Task.FromResult<IEnumerable<StockBalanceWithData>>(((IEnumerable<Expression<Func<StockBalanceWithData, bool>>>) predicates).Aggregate<Expression<Func<StockBalanceWithData, bool>>, IEnumerable<StockBalanceWithData>>(this._balances, (Func<IEnumerable<StockBalanceWithData>, Expression<Func<StockBalanceWithData, bool>>, IEnumerable<StockBalanceWithData>>) ((current, predicate) => current.Where<StockBalanceWithData>(predicate.Compile()))));
  }

  public override void Dispose()
  {
    base.Dispose();
    this._messageToken?.Dispose();
  }
}
