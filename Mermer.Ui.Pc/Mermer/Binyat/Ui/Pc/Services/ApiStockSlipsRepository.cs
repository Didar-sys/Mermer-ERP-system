using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Warehousing.Models;

namespace Mermer.Ui.Pc.Services;

public class ApiStockSlipsRepository : IRepository<StockSlip>, IReadOnlyRepository<StockSlip>, IRepositoryWithFacets<StockSlip>
{
    private readonly RestClient _restClient;
    private const string DocType = "StockSlip";
    private static bool _isSyncing = false;
    private static readonly object _syncLock = new();

    public ApiStockSlipsRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<IEnumerable<StockSlip>> GetAllAsync()
    {
        // 1. Быстро отдаем локальный кэш
        var localSlips = LocalSqliteCache.GetAllDocuments<StockSlip>(DocType)?.ToList() ?? new List<StockSlip>();

        // 2. Если кэш пустой — делаем прямой синхронный запрос, чтобы сразу показать данные
        if (!localSlips.Any())
        {
            try
            {
                var remote = await _restClient.GetAsync<List<StockSlip>>("/api/catalog/slips");
                if (remote != null && remote.Any())
                {
                    foreach (var slip in remote)
                    {
                        LocalSqliteCache.SaveDocument(DocType, slip.Id, slip, isSynced: true);
                    }
                    return remote;
                }
            }
            catch { }
        }
        else
        {
            // 3. Фоновая синхронизация с защитой от параллельных запусков
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
                        // Досылаем несохраненные оффлайн-записи
                        var unsynced = LocalSqliteCache.GetUnsyncedDocuments<StockSlip>(DocType);
                        if (unsynced != null)
                        {
                            foreach (var item in unsynced)
                            {
                                await _restClient.PostAsync("/api/catalog/slips", item.entity);
                                LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                            }
                        }

                        // Обновляем актуальные данные с сервера
                        var remote = await _restClient.GetAsync<List<StockSlip>>("/api/catalog/slips");
                        if (remote != null && remote.Any())
                        {
                            foreach (var slip in remote)
                            {
                                LocalSqliteCache.SaveDocument(DocType, slip.Id, slip, isSynced: true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[StockSlips Sync Error]: {ex.Message}");
                    }
                    finally
                    {
                        lock (_syncLock) { _isSyncing = false; }
                    }
                });
            }
        }

        return localSlips;
    }

    public async Task<StockSlip> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var all = await GetAllAsync();
        return all.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<StockSlip>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<StockSlip>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(s => idSet.Contains(s.Id)).ToList();
    }

    public async Task<IEnumerable<StockSlip>> GetAsync(params Expression<Func<StockSlip, bool>>[] predicates)
    {
        var all = await GetAllAsync();
        var query = all.AsQueryable();
        if (predicates != null)
        {
            foreach (var p in predicates.Where(x => x != null)) query = query.Where(p);
        }
        return query.ToList();
    }

    public async Task<int> CountAsync(params Expression<Func<StockSlip, bool>>[] predicates)
    {
        var result = await GetAsync(predicates);
        return result.Count();
    }

    public async Task SaveAsync(StockSlip entity)
    {
        if (entity == null) return;

        bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
        if (isNew) entity.Id = Guid.NewGuid().ToString();

        LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

        try
        {
            if (isNew) await _restClient.PostAsync("/api/catalog/slips", entity);
            else await _restClient.PutAsync($"/api/catalog/slips/{entity.Id}", entity);

            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
        }
        catch { }
    }

    public async Task CreateAsync(StockSlip entity) => await SaveAsync(entity);
    public async Task UpdateAsync(StockSlip entity) => await SaveAsync(entity);

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        try { await _restClient.DeleteAsync($"/api/catalog/slips/{id}"); } catch { }
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        var dict = new Dictionary<string, Dictionary<string, int>>();
        if (fields != null) foreach (var f in fields) dict[f] = new Dictionary<string, int>();

        try
        {
            var fieldsParam = fields != null && fields.Length > 0 ? string.Join(",", fields) : "Date";
            var apiResult = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/catalog/slips/facets?fields={fieldsParam}");
            if (apiResult != null)
            {
                foreach (var kvp in apiResult) dict[kvp.Key] = kvp.Value;
            }
        }
        catch { }

        return dict;
    }
}