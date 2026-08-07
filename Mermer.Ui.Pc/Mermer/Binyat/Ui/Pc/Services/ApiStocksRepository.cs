using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.StockManagement.Models; // Пространство имен товаров
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Ui.Pc.DTOs;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStocksRepository : IRepository<Stock>, IReadOnlyRepository<Stock>
    {
        private readonly RestClient _restClient;

        public ApiStocksRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

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
            var all = await GetAllAsync();
            return all.FirstOrDefault(s => s.Id == id);
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

        public Task SaveAsync(Stock entity) => Task.CompletedTask;
        public Task CreateAsync(Stock entity) => Task.CompletedTask;
        public Task UpdateAsync(Stock entity) => Task.CompletedTask;
        public Task DeleteAsync(string id) => Task.CompletedTask;
    }
}