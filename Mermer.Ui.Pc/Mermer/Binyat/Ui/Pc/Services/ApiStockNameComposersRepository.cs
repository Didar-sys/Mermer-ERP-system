using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.StockManagement.Models;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockNameComposersRepository :
        IRepository<StockNameComposer>,
        IReadOnlyRepository<StockNameComposer>
    {
        private readonly RestClient _restClient;
        private const string DocType = "StockNameComposer";

        public ApiStockNameComposersRepository(RestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<IEnumerable<StockNameComposer>> GetAllAsync()
        {
            try
            {
                var remote = await _restClient.GetAsync<List<StockNameComposer>>("/api/stock-management/name-composers");
                if (remote != null)
                {
                    foreach (var item in remote) LocalSqliteCache.SaveDocument(DocType, item.Id, item, true);
                    return remote;
                }
            }
            catch { }
            return LocalSqliteCache.GetAllDocuments<StockNameComposer>(DocType)?.ToList() ?? new List<StockNameComposer>();
        }

        public async Task<StockNameComposer> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(x => x.Id == id);
        }

        public async Task<IEnumerable<StockNameComposer>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<StockNameComposer>();
            var all = await GetAllAsync();
            return all.Where(x => ids.Contains(x.Id)).ToList();
        }

        public async Task<IEnumerable<StockNameComposer>> GetAsync(params Expression<Func<StockNameComposer, bool>>[] predicates)
        {
            var query = (await GetAllAsync()).AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates.Where(x => x != null)) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<StockNameComposer, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public async Task SaveAsync(StockNameComposer entity)
        {
            if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, false);
            try
            {
                await _restClient.PostAsync("/api/stock-management/name-composers", entity);
                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, true);
            }
            catch { }
        }

        public Task CreateAsync(StockNameComposer entity) => SaveAsync(entity);
        public Task UpdateAsync(StockNameComposer entity) => SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try { await _restClient.DeleteAsync($"/api/stock-management/name-composers/{id}"); } catch { }
        }
    }
}