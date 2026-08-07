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
    public class ApiWarehousesRepository : IRepository<Warehouse>, IReadOnlyRepository<Warehouse>
    {
        private readonly RestClient _restClient;

        public ApiWarehousesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        // --- ЧТЕНИЕ (IReadOnlyRepository) ---

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            try
            {
                var dtos = await _restClient.GetAsync<List<WarehouseDetailsDto>>("/api/enterprise/warehouses");
                if (dtos == null) return Enumerable.Empty<Warehouse>();

                return dtos.Select(dto => new Warehouse
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    OfficeId = dto.OfficeId,
                    Description = dto.Description
                });
            }
            catch
            {
                return Enumerable.Empty<Warehouse>();
            }
        }

        public async Task<Warehouse> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            try
            {
                var dto = await _restClient.GetAsync<WarehouseDetailsDto>($"/api/enterprise/warehouses/{id}");
                if (dto == null) return null;

                return new Warehouse
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    OfficeId = dto.OfficeId,
                    Description = dto.Description
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Warehouse>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Warehouse>();

            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids);
            return all.Where(w => idSet.Contains(w.Id));
        }

        public async Task<IEnumerable<Warehouse>> GetAsync(params Expression<Func<Warehouse, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();

            if (predicates != null)
            {
                foreach (var predicate in predicates)
                {
                    if (predicate != null)
                        query = query.Where(predicate);
                }
            }

            return query.ToList();
        }

        public async Task<long> CountAsync(params Expression<Func<Warehouse, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        // --- ЗАПИСЬ И ИЗМЕНЕНИЕ (IRepository) ---

        public Task SaveAsync(Warehouse entity) => Task.CompletedTask;

        public Task CreateAsync(Warehouse entity) => Task.CompletedTask;

        public Task UpdateAsync(Warehouse entity) => Task.CompletedTask;

        public Task DeleteAsync(string id) => Task.CompletedTask;

        Task<int> IReadOnlyRepository<Warehouse>.CountAsync(params Expression<Func<Warehouse, bool>>[] predicates)
        {
            throw new NotImplementedException();
        }
    }
}