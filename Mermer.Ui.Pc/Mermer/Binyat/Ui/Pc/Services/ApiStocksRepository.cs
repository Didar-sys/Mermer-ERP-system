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

        public ApiStocksRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        // =========================================================
        // --- МЕТОДЫ СПЕЦИФИЧНЫЕ ДЛЯ IStocksRepository ---
        // =========================================================

        public async Task<IEnumerable<StockInfo>> GetInfoAsync(string additionalPriceCurrencyId, string additionalPriceGroup)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(additionalPriceCurrencyId))
                    queryParams.Add($"additionalPriceCurrencyId={Uri.EscapeDataString(additionalPriceCurrencyId)}");
                if (!string.IsNullOrWhiteSpace(additionalPriceGroup))
                    queryParams.Add($"additionalPriceGroup={Uri.EscapeDataString(additionalPriceGroup)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"/api/stocks{queryString}";

                var result = await _restClient.GetAsync<List<StockInfo>>(url);
                return result ?? Enumerable.Empty<StockInfo>();
            }
            catch
            {
                return Enumerable.Empty<StockInfo>();
            }
        }

        public async Task<IEnumerable<StockInfo>> GetInfoAsync(params string[] stockIds)
        {
            if (stockIds == null || !stockIds.Any())
                return Enumerable.Empty<StockInfo>();

            try
            {
                var all = await GetInfoAsync(null, null);
                var idSet = new HashSet<string>(stockIds);
                return all.Where(x => idSet.Contains(x.Id));
            }
            catch
            {
                return Enumerable.Empty<StockInfo>();
            }
        }

        public async Task<IEnumerable<Stock>> GetListAsync(params string[] stockIds)
        {
            return await GetAsync(stockIds);
        }

        public async Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems)
        {
            if (string.IsNullOrEmpty(mainStockId) || mergeStockIds == null || !mergeStockIds.Any())
                return;

            await _restClient.PostAsync("/api/stocks/merge", new
            {
                mainStockId,
                mergeStockIds,
                disableMergedItems
            });
        }

        // =========================================================
        // --- ЧТЕНИЕ (IReadOnlyRepository / IRepository) ---
        // =========================================================

        public async Task<IEnumerable<Stock>> GetAllAsync()
        {
            try
            {
                var dtos = await _restClient.GetAsync<List<StockDetailsDto>>("/api/stocks");
                if (dtos == null) return Enumerable.Empty<Stock>();

                return dtos.Select(dto => new Stock
                {
                    Id = dto.Id,
                    Name = dto.Name
                });
            }
            catch
            {
                return Enumerable.Empty<Stock>();
            }
        }

        public async Task<Stock> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            try
            {
                return await _restClient.GetAsync<Stock>($"/api/stocks/{id}");
            }
            catch
            {
                var all = await GetAllAsync();
                return all.FirstOrDefault(s => s.Id == id);
            }
        }

        public async Task<IEnumerable<Stock>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Stock>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids);
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

        // =========================================================
        // --- РЕАЛЬНАЯ ЗАПИСЬ (CUD) ---
        // =========================================================

        public async Task CreateAsync(Stock entity)
        {
            if (entity == null) return;
            await _restClient.PostAsync("/api/stocks", entity);
        }

        public async Task UpdateAsync(Stock entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.Id)) return;
            await _restClient.PutAsync($"/api/stocks/{entity.Id}", entity);
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            await _restClient.DeleteAsync($"/api/stocks/{id}");
        }

        public async Task SaveAsync(Stock entity)
        {
            if (entity == null) return;

            if (string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString())
            {
                await CreateAsync(entity);
            }
            else
            {
                await UpdateAsync(entity);
            }
        }

        // =========================================================
        // --- ФАСЕТЫ (IRepositoryWithFacets) ---
        // =========================================================

        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
        {
            var result = new Dictionary<string, Dictionary<string, int>>();
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    result[field] = new Dictionary<string, int>();
                }
            }
            return await Task.FromResult(result);
        }
    }
}