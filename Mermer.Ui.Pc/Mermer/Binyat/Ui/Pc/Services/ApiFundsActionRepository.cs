using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.FundsManagement.Models;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Ui.Pc.DTOs;
using Mermer.Finance.Models;

namespace Mermer.Ui.Pc.Services
{
    public class ApiFundsActionRepository : IRepository<FundsSlip>, IReadOnlyRepository<FundsSlip>
    {
        private readonly RestClient _restClient;

        public ApiFundsActionRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<FundsSlip>> GetAllAsync()
        {
            try
            {
                var dtos = await _restClient.GetAsync<List<FundsActionDto>>("/api/finance/actions");
                if (dtos == null) return Enumerable.Empty<FundsSlip>();

                return dtos.Select(dto => new FundsSlip
                {
                    Id = dto.Id,
                    Code = dto.Code,
                    Date = dto.Date,
                    Description = dto.Description
                });
            }
            catch
            {
                return Enumerable.Empty<FundsSlip>();
            }
        }

        public async Task<FundsSlip> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(a => a.Id == id);
        }

        public async Task<IEnumerable<FundsSlip>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<FundsSlip>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids);
            return all.Where(a => idSet.Contains(a.Id));
        }

        public async Task<IEnumerable<FundsSlip>> GetAsync(params Expression<Func<FundsSlip, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<FundsSlip, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public Task SaveAsync(FundsSlip entity) => Task.CompletedTask;
        public Task CreateAsync(FundsSlip entity) => Task.CompletedTask;
        public Task UpdateAsync(FundsSlip entity) => Task.CompletedTask;
        public Task DeleteAsync(string id) => Task.CompletedTask;
    }
}