using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Ui.Pc.DTOs;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStocksRepository : IStocksRepository
    {
        private readonly RestClient _restClient;
        private const string DocType = "Stock";

        public ApiStocksRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<StockInfo>> GetInfoAsync(string additionalPriceCurrencyId, string additionalPriceGroup)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(additionalPriceCurrencyId)) queryParams.Add($"additionalPriceCurrencyId={Uri.EscapeDataString(additionalPriceCurrencyId)}");
                if (!string.IsNullOrWhiteSpace(additionalPriceGroup)) queryParams.Add($"additionalPriceGroup={Uri.EscapeDataString(additionalPriceGroup)}");

                var url = $"/api/stocks{(queryParams.Any() ? "?" + string.Join("&", queryParams) : "")}";
                return await _restClient.GetAsync<List<StockInfo>>(url) ?? Enumerable.Empty<StockInfo>();
            }
            catch { return Enumerable.Empty<StockInfo>(); }
        }

        public async Task<IEnumerable<StockInfo>> GetInfoAsync(params string[] stockIds)
        {
            if (stockIds == null || !stockIds.Any()) return Enumerable.Empty<StockInfo>();
            try
            {
                var all = await GetInfoAsync(null, null);
                var idSet = new HashSet<string>(stockIds);
                return all.Where(x => idSet.Contains(x.Id));
            }
            catch { return Enumerable.Empty<StockInfo>(); }
        }

        public async Task<IEnumerable<Stock>> GetListAsync(params string[] stockIds) => await GetAsync(stockIds);

        public async Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems)
        {
            if (string.IsNullOrEmpty(mainStockId) || mergeStockIds == null || !mergeStockIds.Any()) return;
            await _restClient.PostAsync("/api/stocks/merge", new { mainStockId, mergeStockIds, disableMergedItems });
        }

        public async Task<IEnumerable<Stock>> GetAllAsync()
        {
            var localStocks = LocalSqliteCache.GetAllDocuments<Stock>(DocType);

            _ = Task.Run(async () =>
            {
                try
                {
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Stock>(DocType);
                    foreach (var (id, stock) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/stocks", stock);
                            LocalSqliteCache.SaveDocument(DocType, id, stock, isSynced: true);
                        }
                        catch { }
                    }

                    var dtos = await _restClient.GetAsync<List<StockDetailsDto>>("/api/stocks");
                    if (dtos != null)
                    {
                        foreach (var dto in dtos)
                        {
                            var s = new Stock { Id = dto.Id, Name = dto.Name };
                            LocalSqliteCache.SaveDocument(DocType, s.Id, s, isSynced: true);
                        }
                    }
                }
                catch { }
            });

            return localStocks;
        }

        public async Task<Stock> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Stock>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Stock>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            return all.Where(s => idSet.Contains(s.Id));
        }

        public async Task<IEnumerable<Stock>> GetAsync(params Expression<Func<Stock, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<Stock, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public async Task SaveAsync(Stock entity)
        {
            if (entity == null) return;
            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            try
            {
                if (isNew) await _restClient.PostAsync("/api/stocks", entity);
                else await _restClient.PutAsync($"/api/stocks/{entity.Id}", entity);
                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
            }
            catch { }
        }

        public async Task CreateAsync(Stock entity) => await SaveAsync(entity);
        public async Task UpdateAsync(Stock entity) => await SaveAsync(entity);
        public async Task DeleteAsync(string id) { try { await _restClient.DeleteAsync($"/api/stocks/{id}"); } catch { } }

        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
        {
            var result = new Dictionary<string, Dictionary<string, int>>();
            if (fields != null)
            {
                foreach (var field in fields) result[field] = new Dictionary<string, int>();
            }
            return await Task.FromResult(result);
        }
    }
}