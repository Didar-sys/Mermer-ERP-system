using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Enterprise.Models;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Ui.Pc.DTOs;

namespace Mermer.Ui.Pc.Services
{
    public class ApiOfficesRepository : IRepository<Office>, IReadOnlyRepository<Office>
    {
        private readonly RestClient _restClient;

        public ApiOfficesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<Office>> GetAllAsync()
        {
            try
            {
                var dtos = await _restClient.GetAsync<List<OfficeDto>>("/api/enterprise/offices");
                if (dtos == null) return Enumerable.Empty<Office>();

                return dtos.Select(dto => new Office
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description
                });
            }
            catch
            {
                return Enumerable.Empty<Office>();
            }
        }

        public async Task<Office> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            try
            {
                var dtos = await GetAllAsync();
                return dtos.FirstOrDefault(o => o.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Office>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Office>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids);
            return all.Where(o => idSet.Contains(o.Id));
        }

        public async Task<IEnumerable<Office>> GetAsync(params Expression<Func<Office, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var predicate in predicates)
                {
                    if (predicate != null) query = query.Where(predicate);
                }
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<Office, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public Task SaveAsync(Office entity) => Task.CompletedTask;
        public Task CreateAsync(Office entity) => Task.CompletedTask;
        public Task UpdateAsync(Office entity) => Task.CompletedTask;
        public Task DeleteAsync(string id) => Task.CompletedTask;
    }
}