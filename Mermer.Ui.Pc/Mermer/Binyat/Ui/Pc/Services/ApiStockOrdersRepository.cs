using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Warehousing.Ordering.Models;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockOrdersRepository :
        IRepository<StockOrder>,
        IReadOnlyRepository<StockOrder>,
        IRepositoryWithFacets<StockOrder>
    {
        private readonly RestClient _restClient;
        private const string DocType = "StockOrder";

        public ApiStockOrdersRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<StockOrder>> GetAllAsync()
        {
            var local = LocalSqliteCache.GetAllDocuments<StockOrder>(DocType)?.ToList() ?? new List<StockOrder>();
            if (!local.Any())
            {
                try
                {
                    var remote = await _restClient.GetAsync<List<StockOrder>>("/api/warehousing/orders");
                    if (remote != null)
                    {
                        foreach (var item in remote) LocalSqliteCache.SaveDocument(DocType, item.Id, item, true);
                        return remote;
                    }
                }
                catch { return local; }
            }
            return local;
        }

        public async Task<StockOrder> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(x => x.Id == id);
        }

        public async Task<IEnumerable<StockOrder>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<StockOrder>();
            var all = await GetAllAsync();
            return all.Where(x => ids.Contains(x.Id)).ToList();
        }

        public async Task<IEnumerable<StockOrder>> GetAsync(params Expression<Func<StockOrder, bool>>[] predicates)
        {
            var query = (await GetAllAsync()).AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates.Where(x => x != null)) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<StockOrder, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public async Task SaveAsync(StockOrder entity)
        {
            if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, false);
            try
            {
                await _restClient.PostAsync("/api/warehousing/orders", entity);
                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, true);
            }
            catch { }
        }

        public Task CreateAsync(StockOrder entity) => SaveAsync(entity);
        public Task UpdateAsync(StockOrder entity) => SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try { await _restClient.DeleteAsync($"/api/warehousing/orders/{id}"); } catch { }
        }

        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
        {
            try
            {
                var query = fields != null && fields.Any() ? "?fields=" + string.Join(",", fields) : "";
                var result = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/warehousing/orders/facets{query}");
                return result ?? new Dictionary<string, Dictionary<string, int>>();
            }
            catch
            {
                var dict = new Dictionary<string, Dictionary<string, int>>();
                if (fields != null) foreach (var f in fields) dict[f] = new Dictionary<string, int>();
                return dict;
            }
        }
    }

    public class ApiAggregatedStockOrdersRepository :
        IRepository<AggregatedStockOrder>,
        IReadOnlyRepository<AggregatedStockOrder>,
        IRepositoryWithFacets<AggregatedStockOrder>
    {
        private readonly RestClient _restClient;
        private const string DocType = "AggregatedStockOrder";

        public ApiAggregatedStockOrdersRepository(RestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<IEnumerable<AggregatedStockOrder>> GetAllAsync()
        {
            try
            {
                var remote = await _restClient.GetAsync<List<AggregatedStockOrder>>("/api/warehousing/aggregated-orders");
                if (remote != null)
                {
                    foreach (var item in remote) LocalSqliteCache.SaveDocument(DocType, item.Id, item, true);
                    return remote;
                }
            }
            catch { }
            return LocalSqliteCache.GetAllDocuments<AggregatedStockOrder>(DocType)?.ToList() ?? new List<AggregatedStockOrder>();
        }

        public async Task<AggregatedStockOrder> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(x => x.Id == id);
        }

        public async Task<IEnumerable<AggregatedStockOrder>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<AggregatedStockOrder>();
            var all = await GetAllAsync();
            return all.Where(x => ids.Contains(x.Id)).ToList();
        }

        public async Task<IEnumerable<AggregatedStockOrder>> GetAsync(params Expression<Func<AggregatedStockOrder, bool>>[] predicates)
        {
            var query = (await GetAllAsync()).AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates.Where(x => x != null)) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<AggregatedStockOrder, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public async Task SaveAsync(AggregatedStockOrder entity)
        {
            if (entity == null) return;
            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, false);
            try
            {
                if (isNew)
                    await _restClient.PostAsync("/api/warehousing/aggregated-orders", entity);
                else
                    await _restClient.PutAsync($"/api/warehousing/aggregated-orders/{entity.Id}", entity);

                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, true);
            }
            catch { }
        }

        public Task CreateAsync(AggregatedStockOrder entity) => SaveAsync(entity);
        public Task UpdateAsync(AggregatedStockOrder entity) => SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try { await _restClient.DeleteAsync($"/api/warehousing/aggregated-orders/{id}"); } catch { }
        }

        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
        {
            try
            {
                var query = fields != null && fields.Any() ? "?fields=" + string.Join(",", fields) : "";
                var result = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/warehousing/aggregated-orders/facets{query}");
                return result ?? new Dictionary<string, Dictionary<string, int>>();
            }
            catch
            {
                return new Dictionary<string, Dictionary<string, int>>();
            }
        }
    }
}