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
        private const string DocType = "Office"; // Идентификатор для SQLite

        public ApiOfficesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        // --- ЧТЕНИЕ (СНАЧАЛА КЭШ, ЗАТЕМ СИНХРОНИЗАЦИЯ) ---
        public async Task<IEnumerable<Office>> GetAllAsync()
        {
            var localOffices = LocalSqliteCache.GetAllDocuments<Office>(DocType);

            _ = Task.Run(async () =>
            {
                try
                {
                    // Досылаем оффлайн-офисы
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Office>(DocType);
                    foreach (var (id, office) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/enterprise/offices", office);
                            LocalSqliteCache.SaveDocument(DocType, id, office, isSynced: true);
                        }
                        catch { }
                    }

                    // Скачиваем свежие офисы
                    var dtos = await _restClient.GetAsync<List<OfficeDto>>("/api/enterprise/offices");
                    if (dtos != null)
                    {
                        foreach (var dto in dtos)
                        {
                            var office = new Office
                            {
                                Id = dto.Id,
                                Name = dto.Name,
                                Description = dto.Description
                            };
                            LocalSqliteCache.SaveDocument(DocType, office.Id, office, isSynced: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Offices Sync Error]: {ex.Message}");
                }
            });

            return localOffices;
        }

        public async Task<Office> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(o => string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Office>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Office>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
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

        // --- ЗАПИСЬ И ИЗМЕНЕНИЕ (CUD) ---

        public async Task SaveAsync(Office entity)
        {
            if (entity == null) return;

            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            try
            {
                if (isNew)
                    await _restClient.PostAsync("/api/enterprise/offices", entity);
                else
                    await _restClient.PutAsync($"/api/enterprise/offices/{entity.Id}", entity);

                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
            }
            catch { }
        }

        public async Task CreateAsync(Office entity) => await SaveAsync(entity);
        public async Task UpdateAsync(Office entity) => await SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try { await _restClient.DeleteAsync($"/api/enterprise/offices/{id}"); } catch { }
        }
    }
}