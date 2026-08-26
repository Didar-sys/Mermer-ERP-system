using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Warehousing.Ordering.Models;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockOrderTemplatesRepository :
        IRepository<StockOrderTemplate>,
        IReadOnlyRepository<StockOrderTemplate>,
        IRepositoryWithFacets<StockOrderTemplate>
    {
        private readonly RestClient _restClient;
        private const string DocType = "StockOrderTemplate";
        private static bool _isSyncing = false;
        private static readonly object _syncLock = new();

        public ApiStockOrderTemplatesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<StockOrderTemplate>> GetAllAsync()
        {
            // 1. Фоновая синхронизация неотправленных записей
            bool shouldSync = false;
            lock (_syncLock)
            {
                if (!_isSyncing)
                {
                    _isSyncing = true;
                    shouldSync = true;
                }
            }

            if (shouldSync)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var unsynced = LocalSqliteCache.GetUnsyncedDocuments<StockOrderTemplate>(DocType);
                        if (unsynced != null && unsynced.Any())
                        {
                            foreach (var item in unsynced)
                            {
                                await _restClient.PostAsync("/api/warehousing/order-templates", item.entity);
                                LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[StockOrderTemplate Background Sync Error]: {ex.Message}");
                    }
                    finally
                    {
                        lock (_syncLock) { _isSyncing = false; }
                    }
                });
            }

            // 2. Получаем данные с сервера
            try
            {
                var remote = await _restClient.GetAsync<List<StockOrderTemplate>>("/api/warehousing/order-templates");
                if (remote != null)
                {
                    foreach (var item in remote)
                    {
                        LocalSqliteCache.SaveDocument(DocType, item.Id, item, isSynced: true);
                    }
                    return remote;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StockOrderTemplate GetAll Error]: {ex.Message}");
            }

            // 3. Фолбэк на локальный кэш
            return LocalSqliteCache.GetAllDocuments<StockOrderTemplate>(DocType)?.ToList() ?? new List<StockOrderTemplate>();
        }

        public async Task<StockOrderTemplate> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            var allLocal = LocalSqliteCache.GetAllDocuments<StockOrderTemplate>(DocType);
            var local = allLocal?.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (local != null) return local;

            try
            {
                var remote = await _restClient.GetAsync<StockOrderTemplate>($"/api/warehousing/order-templates/{id}");
                if (remote != null)
                {
                    LocalSqliteCache.SaveDocument(DocType, remote.Id, remote, isSynced: true);
                    return remote;
                }
            }
            catch { }

            return null;
        }

        public async Task<IEnumerable<StockOrderTemplate>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<StockOrderTemplate>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            return all.Where(x => idSet.Contains(x.Id)).ToList();
        }

        public async Task<IEnumerable<StockOrderTemplate>> GetAsync(params Expression<Func<StockOrderTemplate, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates.Where(x => x != null)) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<StockOrderTemplate, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public async Task SaveAsync(StockOrderTemplate entity)
        {
            if (entity == null) return;
            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            // Сначала сохраняем локально как несинхронизированное
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            try
            {
                if (isNew)
                    await _restClient.PostAsync("/api/warehousing/order-templates", entity);
                else
                    await _restClient.PutAsync($"/api/warehousing/order-templates/{entity.Id}", entity);

                // При успехе помечаем как синхронизированное
                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StockOrderTemplate Save Error]: {ex.Message}");
            }
        }

        public Task CreateAsync(StockOrderTemplate entity) => SaveAsync(entity);
        public Task UpdateAsync(StockOrderTemplate entity) => SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try
            {
                await _restClient.DeleteAsync($"/api/warehousing/order-templates/{id}");
            }
            catch { }
        }

        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
        {
            try
            {
                var query = fields != null && fields.Any() ? "?fields=" + string.Join(",", fields) : "";
                var result = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/warehousing/order-templates/facets{query}");
                return result ?? new Dictionary<string, Dictionary<string, int>>();
            }
            catch
            {
                return new Dictionary<string, Dictionary<string, int>>();
            }
        }
    }
}