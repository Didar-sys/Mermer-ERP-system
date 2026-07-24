using Couchbase.Views;
using Mermer.Core.Couch.Common;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.StockManagement.Services;

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
        _stocksRepository = stocksRepository;
        _currenciesRepository = currenciesRepository;
        _warehousesRepository = warehousesRepository;
        _stockBalancesRepository = stockBalancesRepository;
    }

    public async Task<int> CountAsync(DateTime from, DateTime till, params string[] warehouses)
    {
        // Если переданы конкретные составы — мы должны считать точный результат,
        // чтобы сетка (Grid) не рисовала пустые страницы. Используем готовый GetAsync.
        if (warehouses != null && warehouses.Any())
        {
            var exactRecords = await GetAsync(from, till, warehouses);
            return exactRecords.Count();
        }

        // Если склады не выбраны (считаем глобально) — используем быстрый метод базы
        var records = await GetRecordsAsync<int>(from, till, true);
        return records.Sum();
    }

    public async Task<IEnumerable<DateTime>> GetChangeDatesAsync(DateTime from, DateTime till)
    {
        var records = await GetRecordsAsync<int, DateTime>(from, till, true, 1, x =>
        {
            dynamic key = x.Key;
            return Convert.ToDateTime(key);
        });
        return records.Distinct();
    }

    public async Task<IEnumerable<StockRepriceEffect>> GetAsync(DateTime from, DateTime till, params string[] warehouses)
    {
        var currencies = (await _currenciesRepository.GetAsync()).ToDictionary(x => x.Id, x => x);

        if (warehouses == null || !warehouses.Any())
            warehouses = (await _warehousesRepository.GetAsync()).Select(x => x.Id).ToArray();

        var groupings = (await GetRecordsAsync<StockRepricingInfo>(from, till)).GroupBy(x => x.NextPrice.ValidFrom, x =>
        {
            var rate1 = currencies[x.PrevPrice.CurrencyId].GetRate(x.PrevPrice.ValidFrom);
            decimal num1 = x.PrevPrice.Price * rate1.Multiplier / rate1.Divider;

            var rate2 = currencies[x.NextPrice.CurrencyId].GetRate(x.NextPrice.ValidFrom);
            decimal num2 = x.NextPrice.Price * rate2.Multiplier / rate2.Divider;

            return new
            {
                x.StockId,
                x.StockCode,
                x.StockName,
                PriceChange = num2 - num1,
                PriceChangeDate = x.NextPrice.ValidFrom,
                PriceChangeReason = StockPriceChangeReason.PriceChanged
            };
        });

        var effects = new List<StockRepriceEffect>();

        foreach (var changesGroup in groupings)
        {
            var array = changesGroup.Select(x => x.StockId).Distinct().ToArray();
            var balances = await _stockBalancesRepository.GetAsync(warehouses, array, changesGroup.Key);

            effects.AddRange(balances.GroupJoin(changesGroup, x => x.StockId, x => x.StockId, (balance, priceChanges) =>
                priceChanges.Where(change => change.PriceChange != 0M).Select(change => new StockRepriceEffect
                {
                    StockId = change.StockId,
                    StockCode = change.StockCode,
                    StockName = change.StockName,
                    PriceChange = change.PriceChange,
                    ChangeDate = change.PriceChangeDate,
                    ChangeReason = change.PriceChangeReason,
                    WarehouseId = balance.WarehouseId,
                    Balance = balance.Balance
                })).SelectMany(x => x));
        }

        var currencyChanges = currencies.Values.Where(x => !x.IsDefault)
            .SelectMany(x => x.Rates.Where(r => r.ValidFrom >= from && r.ValidFrom <= till), (c, r) => new
            {
                CurrencyId = c.Id,
                PrevRate = c.Rates.OrderByDescending(pr => pr.ValidFrom).FirstOrDefault(pr => pr.ValidFrom < r.ValidFrom),
                NewRate = r
            })
            .Where(x => x.PrevRate != null)
            .Select(x => new
            {
                x.CurrencyId,
                Date = x.NewRate.ValidFrom,
                Change = x.NewRate.Multiplier / x.NewRate.Divider - x.PrevRate.Multiplier / x.PrevRate.Divider
            });

        var stocks = (await _stocksRepository.GetAsync()).ToList();

        foreach (var currencyChange in currencyChanges)
        {
            var effectedStocks = await Task.Run(() => stocks.Select(x =>
            {
                var stockPrice = x.Prices?.OrderByDescending(p => p.ValidFrom).FirstOrDefault(p => p.ValidFrom <= currencyChange.Date);
                if (stockPrice == null)
                {
                    stockPrice = x.Prices?.OrderBy(p => p.ValidFrom).FirstOrDefault();
                }
                return new
                {
                    StockId = x.Id,
                    StockCode = x.Code,
                    StockName = x.Name,
                    Price = stockPrice
                };
            })
            .Where(x => x.Price != null && x.Price.CurrencyId == currencyChange.CurrencyId)
            .Select(x => new
            {
                x.StockId,
                x.StockCode,
                x.StockName,
                PriceChangeDate = currencyChange.Date,
                PriceChangeReason = StockPriceChangeReason.RateChanged,
                PriceChange = Math.Round(x.Price.Price * currencyChange.Change, 2)
            }).ToList());

            var stockIds = effectedStocks.Select(x => x.StockId).Distinct().ToArray();
            var balances = await _stockBalancesRepository.GetAsync(null, currencyChange.Date, warehouses);

            effects.AddRange(balances.Where(x => stockIds.Contains(x.StockId))
                .GroupJoin(effectedStocks, x => x.StockId, x => x.StockId, (balance, priceChanges) =>
                    priceChanges.Where(change => change.PriceChange != 0M).Select(change => new StockRepriceEffect
                    {
                        StockId = change.StockId,
                        StockCode = change.StockCode,
                        StockName = change.StockName,
                        PriceChange = change.PriceChange,
                        ChangeDate = change.PriceChangeDate,
                        ChangeReason = change.PriceChangeReason,
                        WarehouseId = balance.WarehouseId,
                        Balance = balance.Balance
                    })).SelectMany(x => x));
        }
        return effects;
    }

    private Task<IEnumerable<T>> GetRecordsAsync<T>(DateTime from, DateTime till, bool reduce = false, int groupLevel = 0, Func<ViewRow<T>, T> projector = null)
    {
        return GetRecordsAsync<T, T>(from, till, reduce, groupLevel, projector);
    }

    private Task<IEnumerable<TResult>> GetRecordsAsync<TRow, TResult>(DateTime from, DateTime till, bool reduce = false, int groupLevel = 0, Func<ViewRow<TRow>, TResult> projector = null)
    {
        var array = new Tuple<object, object>[]
        {
            new Tuple<object, object>(from.ToString("o"), till.ToString("o"))
        };
        return GetRecordsAsync<TRow, TResult>("stock-management-reporting", "stock-repricing", array, reduce, groupLevel, projector);
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