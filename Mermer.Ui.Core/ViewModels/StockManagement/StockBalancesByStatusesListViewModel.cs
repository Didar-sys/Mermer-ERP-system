// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockBalancesByStatusesListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Humanizer;
using Mermer.Common.Settings;
using Mermer.Data.Tools.Expressions;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.Mvvm.Messages;
using Mermer.Mvvm.Services;
using Mermer.Services;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

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
        _configurator = configurator;
        _stocksRepository = stocksRepository;
        _balancesRepository = balancesRepository;
        _messageToken = messenger.Subscribe<DocumentModified<StockBalance>>(async m => await Initialize(), MvxReference.Strong);
        Currencies = currencies;
        Warehouses = warehouses;

        Filters = new[]
        {
        new ListFilter
        {
            Title = this["Existing"],
            Tag = "Existing",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["Finished"],
            Tag = "Finished",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["Small Amount"],
            Tag = "Min",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["Over Limit"],
            Tag = "Max",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["All Records"],
            Tag = "All",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
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
        if (!_loaded)
        {
            await Task.WhenAll(Currencies.Initialize(), Warehouses.Initialize());

            AppSettings configAsync = await _configurator.GetConfigAsync<AppSettings>();
            SelectedWarehouseIds = new List<object> { configAsync.DefaultWarehouseId };
            DisplayCurrencyId = Currencies.List.Single(x => x.IsDefault).Id;
            _loaded = true;
        }

        await PreLoadBalances();
        await base.PreLoad();
    }

    private async Task PreLoadBalances()
    {
        if (WarehouseIds.Length == 0)
        {
            _balances = Enumerable.Empty<StockBalanceWithData>();
            return;
        }

        var stocks = await _stocksRepository.GetAsync();
        var balancesList = await _balancesRepository.GetAsync(null, DateTime.Now, WarehouseIds);

        var inner = balancesList
            .GroupBy(x => x.StockId)
            .Select(g => new
            {
                StockId = g.Key,
                Income = g.Sum(x => x.Income),
                Expense = g.Sum(x => x.Expense),
                Balance = g.Sum(x => x.Balance)
            }).ToList();

        var displayCurrency = Currencies.List.Single(x => x.Id == DisplayCurrencyId);
        var displayCurrencyRate = displayCurrency.GetRate();
        int displayCurrencyDecimals = displayCurrency.Decimals;

       
        var query =
            from s in stocks
            join c in Currencies.List on s.CurrencyId equals c.Id
            join b in inner on s.Id equals b.StockId into bGroup
            from sb in bGroup.DefaultIfEmpty()
            let currencyRate = c.GetRate()
            select new StockBalanceWithData
            {
                StockId = s.Id,
                StockCode = s.Code,
                StockName = s.Name,
                StockUnit = s.Unit,
                StockPrice = Math.Round(s.Price * currencyRate.Multiplier / currencyRate.Divider / displayCurrencyRate.Multiplier * displayCurrencyRate.Divider, displayCurrencyDecimals),
                StockGroup = s.Group,
                StockType = s.Type,
                StockTags = s.Tags,
                Income = sb?.Income ?? 0M,
                Expense = sb?.Expense ?? 0M,
                IsExisting = (sb?.Balance ?? 0M) > 0M,
                IsFinished = (sb?.Balance ?? 0M) <= 0M,
                IsOverUsed = (sb?.Balance ?? 0M) < 0M,
                IsFinishing = s.LimitMin.HasValue && (sb?.Balance ?? 0M) < s.LimitMin.Value,
                IsOverLimit = s.LimitMax.HasValue && (sb?.Balance ?? 0M) > s.LimitMax.Value
            };

        _balances = query.ToList();
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
    public ICommand SelectOrViewDetailsCommand => new MvxAsyncCommand(OnSelectOrViewDetailsCommandAsync, () => !IsBusy);

    protected virtual Task OnSelectOrViewDetailsCommandAsync()
    {
        try
        {
            var type = this.GetType();
            var editCmd = type.GetProperty("EditCommand")?.GetValue(this) as ICommand;
            var selectCmd = type.GetProperty("SelectCommand")?.GetValue(this) as ICommand;

            if (selectCmd != null && selectCmd.CanExecute(null))
            {
                selectCmd.Execute(null);
            }
            else if (editCmd != null && editCmd.CanExecute(null))
            {
                editCmd.Execute(null);
            }
        }
        catch (Exception ex)
        {
            // Логирование ошибки, если необходимо
        }

        // Возвращаем успешно завершенный таск напрямую без async/await
        return Task.CompletedTask;
    }
}
