// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.StockManagement.Services.StockRepriceEffectsRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase.Views;
using Microsoft.CSharp.RuntimeBinder;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Data;
using Payhas.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.StockManagement.Services;

public class StockRepriceEffectsRepository : CouchView, IStockRepriceEffectsRepository
{
  private readonly IStocksRepository _stocksRepository;
  private readonly IRepository<Currency> _currenciesRepository;
  private readonly IRepository<Warehouse> _warehousesRepository;
  private readonly IStockBalancesRepository _stockBalancesRepository;

  public StockRepriceEffectsRepository(
    ICouchCluster cluster,
    IStocksRepository stocksRepository,
    IRepository<Currency> currenciesRepository,
    IRepository<Warehouse> warehousesRepository,
    IStockBalancesRepository stockBalancesRepository)
    : base(cluster)
  {
    this._stocksRepository = stocksRepository;
    this._currenciesRepository = currenciesRepository;
    this._warehousesRepository = warehousesRepository;
    this._stockBalancesRepository = stockBalancesRepository;
  }

  public async Task<int> CountAsync(DateTime from, DateTime till)
  {
    return (await this.GetRecordsAsync<int>(from, till, true)).Sum();
  }

  public async Task<IEnumerable<DateTime>> GetChangeDatesAsync(DateTime from, DateTime till)
  {
    return (await this.GetRecordsAsync<int, DateTime>(from, till, true, 1, (Func<ViewRow<int>, DateTime>) (x =>
    {
      // ISSUE: reference to a compiler-generated field
      if (StockRepriceEffectsRepository.\u003C\u003Eo__6.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockRepriceEffectsRepository.\u003C\u003Eo__6.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, DateTime>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (DateTime), typeof (StockRepriceEffectsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      return StockRepriceEffectsRepository.\u003C\u003Eo__6.\u003C\u003Ep__0.Target((CallSite) StockRepriceEffectsRepository.\u003C\u003Eo__6.\u003C\u003Ep__0, x.Key);
    }))).Distinct<DateTime>();
  }

  public async Task<IEnumerable<StockRepriceEffect>> GetAsync(
    DateTime from,
    DateTime till,
    params string[] warehouses)
  {
    Dictionary<string, Currency> currencies = (await this._currenciesRepository.GetAsync()).ToDictionary<Currency, string, Currency>((Func<Currency, string>) (x => x.Id), (Func<Currency, Currency>) (x => x));
    if (warehouses == null || !((IEnumerable<string>) warehouses).Any<string>())
      warehouses = (await this._warehousesRepository.GetAsync()).Select<Warehouse, string>((Func<Warehouse, string>) (x => x.Id)).ToArray<string>();
    IEnumerable<IGrouping<DateTime, \u003C\u003Ef__AnonymousType11<string, string, string, Decimal, DateTime, StockPriceChangeReason>>> groupings = (await this.GetRecordsAsync<StockRepriceEffectsRepository.StockRepricingInfo>(from, till)).GroupBy((Func<StockRepriceEffectsRepository.StockRepricingInfo, DateTime>) (x => x.NextPrice.ValidFrom), x =>
    {
      CurrencyRate rate1 = currencies[x.PrevPrice.CurrencyId].GetRate(new DateTime?(x.PrevPrice.ValidFrom));
      Decimal num1 = x.PrevPrice.Price * rate1.Multiplier / rate1.Divider;
      CurrencyRate rate2 = currencies[x.NextPrice.CurrencyId].GetRate(new DateTime?(x.NextPrice.ValidFrom));
      Decimal num2 = x.NextPrice.Price * rate2.Multiplier / rate2.Divider;
      return new
      {
        StockId = x.StockId,
        StockCode = x.StockCode,
        StockName = x.StockName,
        PriceChange = num2 - num1,
        PriceChangeDate = x.NextPrice.ValidFrom,
        PriceChangeReason = StockPriceChangeReason.PriceChanged
      };
    });
    List<StockRepriceEffect> effects = new List<StockRepriceEffect>();
    foreach (IGrouping<DateTime, \u003C\u003Ef__AnonymousType11<string, string, string, Decimal, DateTime, StockPriceChangeReason>> changesGroup in groupings)
    {
      string[] array = changesGroup.Select(x => x.StockId).Distinct<string>().ToArray<string>();
      effects.AddRange((await this._stockBalancesRepository.GetAsync(warehouses, array, new DateTime?(changesGroup.Key))).GroupJoin(changesGroup, (Func<StockBalance, string>) (x => x.StockId), x => x.StockId, (balance, priceChanges) => priceChanges.Where(change => change.PriceChange != 0M).Select(change =>
      {
        return new StockRepriceEffect()
        {
          StockId = change.StockId,
          StockCode = change.StockCode,
          StockName = change.StockName,
          PriceChange = change.PriceChange,
          ChangeDate = change.PriceChangeDate,
          ChangeReason = change.PriceChangeReason,
          WarehouseId = balance.WarehouseId,
          Balance = balance.Balance
        };
      })).SelectMany<IEnumerable<StockRepriceEffect>, StockRepriceEffect>((Func<IEnumerable<StockRepriceEffect>, IEnumerable<StockRepriceEffect>>) (x => x)));
    }
    IEnumerable<\u003C\u003Ef__AnonymousType13<string, DateTime, Decimal>> currencyChanges = currencies.Values.Where<Currency>((Func<Currency, bool>) (x => !x.IsDefault)).SelectMany((Func<Currency, IEnumerable<CurrencyRate>>) (x => x.Rates.Where<CurrencyRate>((Func<CurrencyRate, bool>) (r => r.ValidFrom >= from && r.ValidFrom <= till))), (c, r) => new
    {
      CurrencyId = c.Id,
      PrevRate = c.Rates.OrderByDescending<CurrencyRate, DateTime>((Func<CurrencyRate, DateTime>) (pr => pr.ValidFrom)).FirstOrDefault<CurrencyRate>((Func<CurrencyRate, bool>) (pr => pr.ValidFrom < r.ValidFrom)),
      NewRate = r
    }).Where(x => x.PrevRate != null).Select(x => new
    {
      CurrencyId = x.CurrencyId,
      Date = x.NewRate.ValidFrom,
      Change = x.NewRate.Multiplier / x.NewRate.Divider - x.PrevRate.Multiplier / x.PrevRate.Divider
    });
    List<Stock> stocks = (await this._stocksRepository.GetAsync()).ToList<Stock>();
    foreach (var data in currencyChanges)
    {
      var currencyChange = data;
      List<\u003C\u003Ef__AnonymousType15<string, string, string, DateTime, StockPriceChangeReason, Decimal>> effectedStocks = await Task.Run<List<\u003C\u003Ef__AnonymousType15<string, string, string, DateTime, StockPriceChangeReason, Decimal>>>(() => stocks.Select(x =>
      {
        string id = x.Id;
        string code = x.Code;
        string name = x.Name;
        WatchedObservableCollection<StockPrice> prices1 = x.Prices;
        StockPrice stockPrice = prices1 != null ? prices1.OrderByDescending<StockPrice, DateTime>((Func<StockPrice, DateTime>) (p => p.ValidFrom)).FirstOrDefault<StockPrice>((Func<StockPrice, bool>) (p => p.ValidFrom <= currencyChange.Date)) : (StockPrice) null;
        if (stockPrice == null)
        {
          WatchedObservableCollection<StockPrice> prices2 = x.Prices;
          stockPrice = prices2 != null ? prices2.OrderBy<StockPrice, DateTime>((Func<StockPrice, DateTime>) (p => p.ValidFrom)).FirstOrDefault<StockPrice>() : (StockPrice) null;
        }
        return new
        {
          StockId = id,
          StockCode = code,
          StockName = name,
          Price = stockPrice
        };
      }).Where(x => x.Price.CurrencyId == currencyChange.CurrencyId).Select(x => new
      {
        StockId = x.StockId,
        StockCode = x.StockCode,
        StockName = x.StockName,
        PriceChangeDate = currencyChange.Date,
        PriceChangeReason = StockPriceChangeReason.RateChanged,
        PriceChange = Math.Round(x.Price.Price * currencyChange.Change, 2)
      }).ToList());
      string[] stockIds = effectedStocks.Select(x => x.StockId).Distinct<string>().ToArray<string>();
      effects.AddRange((await this._stockBalancesRepository.GetAsync((string) null, currencyChange.Date, warehouses)).Where<StockBalance>((Func<StockBalance, bool>) (x => ((IEnumerable<string>) stockIds).Contains<string>(x.StockId))).GroupJoin(effectedStocks, (Func<StockBalance, string>) (x => x.StockId), x => x.StockId, (balance, priceChanges) => priceChanges.Where(change => change.PriceChange != 0M).Select(change =>
      {
        return new StockRepriceEffect()
        {
          StockId = change.StockId,
          StockCode = change.StockCode,
          StockName = change.StockName,
          PriceChange = change.PriceChange,
          ChangeDate = change.PriceChangeDate,
          ChangeReason = change.PriceChangeReason,
          WarehouseId = balance.WarehouseId,
          Balance = balance.Balance
        };
      })).SelectMany<IEnumerable<StockRepriceEffect>, StockRepriceEffect>((Func<IEnumerable<StockRepriceEffect>, IEnumerable<StockRepriceEffect>>) (x => x)));
      effectedStocks = null;
    }
    IEnumerable<StockRepriceEffect> async = (IEnumerable<StockRepriceEffect>) effects;
    effects = (List<StockRepriceEffect>) null;
    currencyChanges = null;
    return async;
  }

  private Task<IEnumerable<T>> GetRecordsAsync<T>(
    DateTime from,
    DateTime till,
    bool reduce = false,
    int groupLevel = 0,
    Func<ViewRow<T>, T> projector = null)
  {
    return this.GetRecordsAsync<T, T>(from, till, reduce, groupLevel, projector);
  }

  private Task<IEnumerable<TResult>> GetRecordsAsync<TRow, TResult>(
    DateTime from,
    DateTime till,
    bool reduce = false,
    int groupLevel = 0,
    Func<ViewRow<TRow>, TResult> projector = null)
  {
    return this.GetRecordsAsync<TRow, TResult>("stock-management-reporting", "stock-repricing", new Tuple<object, object>[1]
    {
      new Tuple<object, object>((object) from.ToString("o"), (object) till.ToString("o"))
    }, (reduce ? 1 : 0) != 0, groupLevel, projector);
  }

  internal class StockRepricingInfo
  {
    public string StockId { get; set; }

    public string StockCode { get; set; }

    public string StockName { get; set; }

    public StockPrice PrevPrice { get; set; }

    public StockPrice NextPrice { get; set; }
  }
}
