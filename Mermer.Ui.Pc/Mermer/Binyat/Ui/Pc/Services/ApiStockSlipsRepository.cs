using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.StockManagement.Models;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Ui.Pc.DTOs;
using Mermer.Warehousing.Models;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockSlipsRepository : IRepository<StockSlip>, IReadOnlyRepository<StockSlip>
    {
        private readonly RestClient _restClient;

        public ApiStockSlipsRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<StockSlip>> GetAllAsync()
        {
            try
            {
                var dtos = await _restClient.GetAsync<List<StockSlipDto>>("/api/catalog/slips");
                if (dtos == null) return Enumerable.Empty<StockSlip>();

                return dtos.Select(dto => new StockSlip
                {
                    Id = dto.Id,
                    Code = dto.Code,
                    Date = dto.Date,
                    IsCompleted = dto.IsCompleted,
                    Description = dto.Description
                });
            }
            catch
            {
                return Enumerable.Empty<StockSlip>();
            }
        }

        public async Task<StockSlip> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(s => s.Id == id);
        }

        public async Task<IEnumerable<StockSlip>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<StockSlip>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids);
            return all.Where(s => idSet.Contains(s.Id));
        }

        public async Task<IEnumerable<StockSlip>> GetAsync(params Expression<Func<StockSlip, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<StockSlip, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public Task SaveAsync(StockSlip entity) => Task.CompletedTask;
        public Task CreateAsync(StockSlip entity) => Task.CompletedTask;
        public Task UpdateAsync(StockSlip entity) => Task.CompletedTask;
        public Task DeleteAsync(string id) => Task.CompletedTask;
    }
}