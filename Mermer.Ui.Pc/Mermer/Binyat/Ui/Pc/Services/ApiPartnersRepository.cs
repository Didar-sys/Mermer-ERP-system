using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.CRM.Models;
using Mermer.Data.Storage;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services
{
    public class ApiPartnersRepository : IRepository<Partner>, IReadOnlyRepository<Partner>, IRepositoryWithFacets<Partner>
    {
        private readonly RestClient _restClient;

        public ApiPartnersRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        // --- ЧТЕНИЕ ---
        public async Task<IEnumerable<Partner>> GetAllAsync()
        {
            try
            {
                var partners = await _restClient.GetAsync<List<Partner>>("/api/catalog/partners");
                return partners ?? Enumerable.Empty<Partner>();
            }
            catch
            {
                return Enumerable.Empty<Partner>();
            }
        }

        public async Task<Partner> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return await _restClient.GetAsync<Partner>($"/api/catalog/partners/{id}");
        }

        public async Task<IEnumerable<Partner>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Partner>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids);
            return all.Where(p => idSet.Contains(p.Id));
        }

        public async Task<IEnumerable<Partner>> GetAsync(params Expression<Func<Partner, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<Partner, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        // =========================================================
        // --- РЕАЛЬНАЯ ЗАПИСЬ (CUD) ---
        // =========================================================
        public async Task CreateAsync(Partner entity)
        {
            if (entity == null) return;
            await _restClient.PostAsync("/api/catalog/partners", entity);
        }

        public async Task UpdateAsync(Partner entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.Id)) return;
            await _restClient.PutAsync($"/api/catalog/partners/{entity.Id}", entity);
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            await _restClient.DeleteAsync($"/api/catalog/partners/{id}");
        }

        public async Task SaveAsync(Partner entity)
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