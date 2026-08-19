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
    public class ApiStockOrderTemplatesRepository :
        IRepository<StockOrderTemplate>,
        IReadOnlyRepository<StockOrderTemplate>,
        IRepositoryWithFacets<StockOrderTemplate>
    {
        private readonly RestClient _restClient;
        private const string DocType = "StockOrderTemplate";

        public ApiStockOrderTemplatesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<StockOrderTemplate>> GetAllAsync()
        {
            try
            {
                var remote = await _restClient.GetAsync<List<StockOrderTemplate>>("/api/warehousing/order-templates");
                if (remote != null)
                {
                    foreach (var item in remote) LocalSqliteCache.SaveDocument(DocType, item.Id, item, true);
                    return remote;
                }
            }
            catch { }
            return LocalSqliteCache.GetAllDocuments<StockOrderTemplate>(DocType)?.ToList() ?? new List<StockOrderTemplate>();
        }

        public async Task<StockOrderTemplate> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(x => x.Id == id);
        }

        public async Task<IEnumerable<StockOrderTemplate>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<StockOrderTemplate>();
            var all = await GetAllAsync();
            return all.Where(x => ids.Contains(x.Id)).ToList();
        }

        public async Task<IEnumerable<StockOrderTemplate>> GetAsync(params Expression<Func<StockOrderTemplate, bool>>[] predicates)
        {
            var query = (await GetAllAsync()).AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates.Where(x => x != null)) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<StockOrderTemplate, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public async Task SaveAsync(StockOrderTemplate entity)
        {
            if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, false);
            try
            {
                await _restClient.PostAsync("/api/warehousing/order-templates", entity);
                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, true);
            }
            catch { }
        }

        public Task CreateAsync(StockOrderTemplate entity) => SaveAsync(entity);
        public Task UpdateAsync(StockOrderTemplate entity) => SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try
            {
                await _restClient.DeleteAsync($"/api/warehousing/order-templates/{id}");
            }
            catch { }
        }

        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
        {
            try
            {
                var query = fields != null && fields.Any() ? "?fields=" + string.Join(",", fields) : "";
                var result = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/warehousing/order-templates/facets{query}");
                return result ?? new Dictionary<string, Dictionary<string, int>>();
            }
            catch
            {
                return new Dictionary<string, Dictionary<string, int>>();
            }
        }
    }
}