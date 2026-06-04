using Couchbase.Views;
using Mermer.Core.Couch.Common;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.StockManagement.Services;

public class StockTurnoverDataRepository : CouchView, IStockTurnoverDataRepository
{
    private readonly IStocksRepository _stocksRepository;

    public StockTurnoverDataRepository(ICouchCluster cluster, IStocksRepository stocksRepository)
        : base(cluster)
    {
        _stocksRepository = stocksRepository;
    }

    public async Task<IEnumerable<StockTurnoverData>> GetAsync(string warehouseId = null)
    {
        var startEndKeys = string.IsNullOrEmpty(warehouseId)
            ? null
            : new[]
            {
                new Tuple<object, object>(
                    new[] { warehouseId, "0" },
                    new[] { warehouseId, "zzz" }
                )
            };

        var list = (await GetRecordsAsync<StockTurnoverData>("stock-management-reporting", "stock-turnovers", startEndKeys, true, 2, x =>
        {
            var turnover = x.Value;
            dynamic key = x.Key;
            turnover.WarehouseId = (string)key[0];
            turnover.StockId = (string)key[1];
            return turnover;
        })).ToArray();

        var stockIds = list.Select(x => x.StockId).Distinct().ToArray();

        // 1. БЕЗОПАСНЫЙ СЛОВАРЬ: отбрасываем null-значения перед конвертацией
        var stocks = (await _stocksRepository.GetListAsync(stockIds))
            .Where(x => x != null)
            .ToDictionary(x => x.Id, x => x);

        return list.Select(x =>
        {
            // 2. БЕЗОПАСНОЕ ПРИСВОЕНИЕ: проверяем, нашелся ли товар
            if (stocks.TryGetValue(x.StockId, out var stock))
            {
                x.StockCode = stock.Code;
                x.StockName = stock.Name;
                x.StockGroup = stock.Group;
                x.StockType = stock.Type;
                x.StockTags = stock.Tags;
            }
            else
            {
                // Если товара больше нет в базе, чтобы таблица не была пустой:
                x.StockCode = "N/A";
                x.StockName = "Удаленный товар";
            }
            return x;
        });
    }
}