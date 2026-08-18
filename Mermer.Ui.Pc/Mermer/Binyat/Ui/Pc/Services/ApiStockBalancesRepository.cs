using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mermer.Http;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockBalancesRepository : IStockBalancesRepository
    {
        private readonly RestClient _restClient;

        public ApiStockBalancesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public Task<IEnumerable<StockBalance>> GetAsync(string stockId, DateTime date, params string[] warehouses)
            => Task.FromResult(Enumerable.Empty<StockBalance>());

        public Task<IEnumerable<StockBalance>> GetAsync(string warehouseId, string[] stockIds, DateTime? date = null)
            => Task.FromResult(Enumerable.Empty<StockBalance>());

        public Task<IEnumerable<StockBalance>> GetAsync(string[] warehouseIds, string[] stockIds, DateTime? date = null)
            => Task.FromResult(Enumerable.Empty<StockBalance>());

        public Task<IEnumerable<StockBalance>> GetAsync(string warehouseId, (string stockId, DateTime? balanceDate)[] stockBalanceDates)
            => Task.FromResult(Enumerable.Empty<StockBalance>());

        public Task<IEnumerable<StockBalance>> GetAsync(string[] warehouseIds, (string stockId, DateTime? balanceDate)[] stockBalanceDates)
            => Task.FromResult(Enumerable.Empty<StockBalance>());

        public Task<IEnumerable<StockBalanceWithCodeAndName>> GetAsync(string warehouseId, string[] stockIds, string excludedTransactionId)
            => Task.FromResult(Enumerable.Empty<StockBalanceWithCodeAndName>());

        public async Task<IEnumerable<StockBalanceByTypeWithBalanceAndData>> GetByTypeAsync(string[] warehouseIds, string stockId, DateTime dateFrom, DateTime dateTill, bool aggregate)
        {
            try
            {
                // ИСПРАВЛЕНИЕ: Форматируем даты в безопасный для URL UTC-стандарт
                var query = $"?dateFrom={dateFrom.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}&dateTill={dateTill.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}&aggregate={aggregate}";
                if (!string.IsNullOrEmpty(stockId)) query += $"&stockId={stockId}";
                if (warehouseIds != null) foreach (var w in warehouseIds) query += $"&warehouseId={w}";

                var remote = await _restClient.GetAsync<List<StockBalanceByTypeWithBalanceAndData>>($"/api/stock-balances/by-type{query}");
                return remote ?? new List<StockBalanceByTypeWithBalanceAndData>();
            }
            catch { return new List<StockBalanceByTypeWithBalanceAndData>(); }
        }

        public async Task<IEnumerable<StockBalanceByWarehouses>> GetByDateAndWarehousesAsync(DateTime date, IEnumerable<string> warehouseIds, string displayCurrencyId, IEnumerable<string> stockIds = null)
        {
            try
            {
                // ИСПРАВЛЕНИЕ: Форматируем даты в безопасный для URL UTC-стандарт
                var query = $"?date={date.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}&displayCurrencyId={displayCurrencyId}";
                if (warehouseIds != null) foreach (var w in warehouseIds) query += $"&warehouseId={w}";
                if (stockIds != null) foreach (var s in stockIds) query += $"&stockId={s}";

                var remote = await _restClient.GetAsync<List<StockBalanceByWarehouses>>($"/api/stock-balances/by-date-warehouses{query}");
                return remote ?? new List<StockBalanceByWarehouses>();
            }
            catch { return new List<StockBalanceByWarehouses>(); }
        }
    }

    public class ApiStockBalancesAggregatedRepository : IStockBalancesAggregatedRepository
    {
        private readonly RestClient _restClient;

        public ApiStockBalancesAggregatedRepository(RestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<StockBalanceAggregated> GetByTypeAggregatedAsync(string[] warehouseIds, DateTime dateFrom, DateTime dateTill)
        {
            try
            {
                // ИСПРАВЛЕНИЕ: Форматируем даты в безопасный UTC-формат
                var query = $"?dateFrom={dateFrom.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}&dateTill={dateTill.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}";

                if (warehouseIds != null) foreach (var w in warehouseIds) query += $"&warehouseId={w}";
                var remote = await _restClient.GetAsync<StockBalanceAggregated>($"/api/stock-balances/aggregated{query}");
                return remote ?? new StockBalanceAggregated { Lines = Array.Empty<StockBalanceAggregatedLine>() };
            }
            catch { return new StockBalanceAggregated { Lines = Array.Empty<StockBalanceAggregatedLine>() }; }
        }
    }
}