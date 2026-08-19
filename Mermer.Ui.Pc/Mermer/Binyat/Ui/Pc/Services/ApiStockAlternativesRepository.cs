using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockAlternativesRepository : IStockAlternativesRepository
    {
        private readonly RestClient _restClient;
        private const string DocType = "StockAlternative";

        public ApiStockAlternativesRepository(RestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<IEnumerable<StockAlternative>> GetAllAsync()
        {
            try
            {
                var remote = await _restClient.GetAsync<List<StockAlternative>>("/api/stock-management/alternatives");
                if (remote != null)
                {
                    foreach (var item in remote) LocalSqliteCache.SaveDocument(DocType, item.Id, item, true);
                    return remote;
                }
            }
            catch { }
            return LocalSqliteCache.GetAllDocuments<StockAlternative>(DocType)?.ToList() ?? new List<StockAlternative>();
        }

        public async Task<StockAlternative> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return (await GetAllAsync()).FirstOrDefault(x => x.Id == id);
        }

        public async Task<IEnumerable<StockAlternative>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<StockAlternative>();
            return (await GetAllAsync()).Where(x => ids.Contains(x.Id)).ToList();
        }

        public async Task<IEnumerable<StockAlternative>> GetAsync(params Expression<Func<StockAlternative, bool>>[] predicates)
        {
            var query = (await GetAllAsync()).AsQueryable();
            if (predicates != null) foreach (var p in predicates.Where(x => x != null)) query = query.Where(p);
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<StockAlternative, bool>>[] predicates) => (await GetAsync(predicates)).Count();

        public async Task SaveAsync(StockAlternative entity)
        {
            if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, false);
            try
            {
                await _restClient.PostAsync("/api/stock-management/alternatives", entity);
                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, true);
            }
            catch { }
        }

        public Task CreateAsync(StockAlternative entity) => SaveAsync(entity);
        public Task UpdateAsync(StockAlternative entity) => SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try
            {
                await _restClient.DeleteAsync($"/api/stock-management/alternatives/{id}");
            }
            catch { }
        }

        public async Task<SingleStockAlternative> GetAlternativesAsync(string stockId)
        {
            if (string.IsNullOrEmpty(stockId)) return new SingleStockAlternative { StockId = stockId, Alternatives = Array.Empty<string>() };
            try
            {
                var result = await _restClient.GetAsync<SingleStockAlternative>($"/api/stock-management/alternatives/for-stock/{stockId}");
                return result ?? new SingleStockAlternative { StockId = stockId, Alternatives = Array.Empty<string>() };
            }
            catch
            {
                return new SingleStockAlternative { StockId = stockId, Alternatives = Array.Empty<string>() };
            }
        }
    }
}