using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.CRM.Models; // Пространство имен контрагентов
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Ui.Pc.DTOs;

namespace Mermer.Ui.Pc.Services
{
    public class ApiPartnersRepository : IRepository<Partner>, IReadOnlyRepository<Partner>
    {
        private readonly RestClient _restClient;

        public ApiPartnersRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<Partner>> GetAllAsync()
        {
            try
            {
                var dtos = await _restClient.GetAsync<List<PartnerDetailsDto>>("/api/catalog/partners");
                if (dtos == null) return Enumerable.Empty<Partner>();

                return dtos.Select(dto => new Partner
                {
                    Id = dto.Id,
                    Name = dto.Name
                });
            }
            catch
            {
                return Enumerable.Empty<Partner>();
            }
        }

        public async Task<Partner> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(p => p.Id == id);
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

        public Task SaveAsync(Partner entity) => Task.CompletedTask;
        public Task CreateAsync(Partner entity) => Task.CompletedTask;
        public Task UpdateAsync(Partner entity) => Task.CompletedTask;
        public Task DeleteAsync(string id) => Task.CompletedTask;
    }
}