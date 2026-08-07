using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.FundsManagement.Models;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Ui.Pc.DTOs;

namespace Mermer.Ui.Pc.Services
{
    public class ApiCurrenciesRepository : IRepository<Currency>, IReadOnlyRepository<Currency>
    {
        private readonly RestClient _restClient;

        public ApiCurrenciesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<Currency>> GetAllAsync()
        {
            try
            {
                var dtos = await _restClient.GetAsync<List<CurrencyDto>>("/api/enterprise/currencies");
                if (dtos == null) return Enumerable.Empty<Currency>();

                return dtos.Select(dto => new Currency
                {
                    Id = dto.Id,
                    Name = dto.Name
                });
            }
            catch
            {
                return Enumerable.Empty<Currency>();
            }
        }

        public async Task<Currency> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(c => c.Id == id);
        }

        public async Task<IEnumerable<Currency>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Currency>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids);
            return all.Where(c => idSet.Contains(c.Id));
        }

        public async Task<IEnumerable<Currency>> GetAsync(params Expression<Func<Currency, bool>>[] predicates)
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

        public async Task<int> CountAsync(params Expression<Func<Currency, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public Task SaveAsync(Currency entity) => Task.CompletedTask;
        public Task CreateAsync(Currency entity) => Task.CompletedTask;
        public Task UpdateAsync(Currency entity) => Task.CompletedTask;
        public Task DeleteAsync(string id) => Task.CompletedTask;
    }
}