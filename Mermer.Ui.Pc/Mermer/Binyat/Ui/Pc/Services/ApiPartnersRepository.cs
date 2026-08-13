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

        // --- ЧТЕНИЕ (CACHE-FIRST) ---
        public async Task<IEnumerable<Partner>> GetAllAsync()
        {
            var local = LocalSqliteCache.GetAllDocuments<Partner>(DocType).ToList();

            _ = Task.Run(async () =>
            {
                try
                {
                    // 1. Досылаем несинхронизированных партнеров из оффлайна
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Partner>(DocType);
                    foreach (var (id, partner) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/partners", partner);
                            LocalSqliteCache.SaveDocument(DocType, id, partner, isSynced: true);
                        }
                        catch { }
                    }

                    // 2. Скачиваем свежий список с сервера
                    var remote = await _restClient.GetAsync<List<Partner>>("/api/partners");
                    if (remote != null)
                    {
                        foreach (var partner in remote)
                        {
                            LocalSqliteCache.SaveDocument(DocType, partner.Id, partner, isSynced: true);
                        }
                    }
                }
                catch { }
            });

            return local;
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

        // --- СОХРАНЕНИЕ (ОФФЛАЙН-ПЕРВЫМ ДЕЛОМ) ---
        public async Task SaveAsync(Partner entity)
        {
            Log("=== [ApiPartnersRepository] ENTER SaveAsync ===");
            if (entity == null)
            {
                Log("=== [ApiPartnersRepository] ENTITY IS NULL ===");
                return;
            }

            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            try
            {
                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);
                Log($"=== [ApiPartnersRepository] SUCCESS SQLITE SAVE: ID={entity.Id}, Name={entity.Name} ===");
            }
            catch (Exception ex)
            {
                Log($"=== [ApiPartnersRepository] SQLITE ERROR: {ex.Message} ===");
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    if (isNew)
                        await _restClient.PostAsync("/api/partners", entity);
                    else
                        await _restClient.PutAsync($"/api/partners/{entity.Id}", entity);

                    LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
                    Log("=== [ApiPartnersRepository] API SYNC SUCCESS ===");
                }
                catch (Exception ex)
                {
                    Log($"[Partner Save API Warning]: {ex.Message}");
                }
            });
        }

        private static void Log(string message)
        {
            try
            {
                System.IO.File.AppendAllText(@"C:\Users\Public\mermer_debug.log", $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch { }
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

        // --- РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА ФАСЕТОВ ДЛЯ ФИЛЬТРОВ ---
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