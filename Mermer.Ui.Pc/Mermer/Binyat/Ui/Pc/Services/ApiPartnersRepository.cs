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
        private const string DocType = "Partner";

        public ApiPartnersRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<Partner>> GetAllAsync()
        {
            var localPartners = LocalSqliteCache.GetAllDocuments<Partner>(DocType);

            _ = Task.Run(async () =>
            {
                try
                {
                    // 1. ДОСЫЛАЕМ СТАРЫЕ НЕСИНХРОНИЗИРОВАННЫЕ ЗАПИСИ (is_synced = 0)
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Partner>(DocType);
                    foreach (var (id, partner) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/partners", partner);
                            // После успешной отправки меняем 0 на 1 в SQLite!
                            LocalSqliteCache.SaveDocument(DocType, id, partner, isSynced: true);
                        }
                        catch { /* Если API временно недоступен — попробует в следующий раз */ }
                    }

                    // 2. ЗАТЯГИВАЕМ СВЕЖИЕ ЗАПИСИ С СЕРВЕРА
                    var remotePartners = await _restClient.GetAsync<List<Partner>>("/api/partners");
                    if (remotePartners != null)
                    {
                        foreach (var partner in remotePartners)
                        {
                            LocalSqliteCache.SaveDocument(DocType, partner.Id, partner, isSynced: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Partner Sync Error]: {ex.Message}");
                }
            });

            return localPartners;
        }

        public async Task<Partner> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Partner>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Partner>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
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

        public async Task SaveAsync(Partner entity)
        {
            if (entity == null) return;

            if (string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString())
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            try
            {
                await _restClient.PostAsync("/api/partners", entity);
                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Partner Save API Error]: {ex.Message}");
            }
        }

        public async Task CreateAsync(Partner entity) => await SaveAsync(entity);
        public async Task UpdateAsync(Partner entity) => await SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try
            {
                await _restClient.DeleteAsync($"/api/partners/{id}");
            }
            catch { }
        }

        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
        {
            var result = new Dictionary<string, Dictionary<string, int>>();
            if (fields != null)
            {
                foreach (var field in fields) result[field] = new Dictionary<string, int>();
            }
            return await Task.FromResult(result);
        }
    }
}