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

        // --- ЧТЕНИЕ ---

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

        public async Task<int> CountAsync(params Expression<Func<Warehouse, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        // --- ЗАПИСЬ И ИЗМЕНЕНИЕ (CUD) ---

        public async Task CreateAsync(Warehouse entity)
        {
            if (entity == null) return;
            await _restClient.PostAsync("/api/enterprise/warehouses", entity);
        }

        public async Task UpdateAsync(Warehouse entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.Id)) return;
            await _restClient.PutAsync($"/api/enterprise/warehouses/{entity.Id}", entity);
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            await _restClient.DeleteAsync($"/api/enterprise/warehouses/{id}");
        }

        public async Task SaveAsync(Warehouse entity)
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
    }
}