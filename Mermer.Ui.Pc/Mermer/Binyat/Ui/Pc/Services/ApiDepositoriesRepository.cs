using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services
{
    public class ApiDepositoriesRepository :
        IRepository<Depository>,
        IReadOnlyRepository<Depository>,
        IRepositoryWithFacets<Depository>
    {
        private readonly RestClient _restClient;

        public ApiDepositoriesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<Depository>> GetAsync()
        {
            return await GetAllAsync();
        }

        public async Task<IEnumerable<Depository>> GetAllAsync()
        {
            try
            {
                var result = await _restClient.GetAsync<List<Depository>>("/api/depositories");
                return result ?? Enumerable.Empty<Depository>();
            }
            catch
            {
                return Enumerable.Empty<Depository>();
            }
        }

        public async Task<Depository> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Depository>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Depository>();
            var all = await GetAllAsync();
            var set = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            return all.Where(d => set.Contains(d.Id));
        }

        public async Task<IEnumerable<Depository>> GetAsync(params Expression<Func<Depository, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<Depository, bool>>[] predicates)
        {
            var res = await GetAsync(predicates);
            return res.Count();
        }

        public Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] facetFields)
        {
            var result = new Dictionary<string, Dictionary<string, int>>();
            if (facetFields != null)
            {
                foreach (var field in facetFields)
                    result[field] = new Dictionary<string, int>();
            }
            return Task.FromResult(result);
        }

        public Task SaveAsync(Depository entity) => Task.CompletedTask;
        public Task CreateAsync(Depository entity) => Task.CompletedTask;
        public Task UpdateAsync(Depository entity) => Task.CompletedTask;
        public Task DeleteAsync(string id) => Task.CompletedTask;
    }
}