using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Commerce.Models;
using Mermer.Data.Storage;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiBillsRepository : IRepositoryWithFacets<Bill>, IRepository<Bill>, IReadOnlyRepository<Bill>
{
    private readonly RestClient _restClient;
    private const string DocType = "Bill";

    public ApiBillsRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<Bill> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        var allLocal = LocalSqliteCache.GetAllDocuments<Bill>(DocType);
        var local = allLocal?.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        if (local != null) return local;

        try
        {
            var remote = await _restClient.GetAsync<Bill>($"/api/bills/{id}");
            if (remote != null)
            {
                LocalSqliteCache.SaveDocument(DocType, remote.Id, remote, isSynced: true);
                return remote;
            }
        }
        catch { }

        return null;
    }

    public async Task<IEnumerable<Bill>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<Bill>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(b => idSet.Contains(b.Id)).ToList();
    }

    public async Task<IEnumerable<Bill>> GetAsync(params Expression<Func<Bill, bool>>[] predicates)
    {
        var all = await GetAllAsync();
        var query = all.AsQueryable();

        if (predicates != null && predicates.Any())
        {
            foreach (var predicate in predicates.Where(p => p != null))
            {
                query = query.Where(predicate);
            }
        }

        return query.ToList();
    }

    private async Task<IEnumerable<Bill>> GetAllAsync()
    {
        // 1. Досылаем на сервер всё, что висит со статусом isSynced == false
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Bill>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PostAsync("/api/bills", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        // 2. Отдаем локальный кэш
        var localItems = LocalSqliteCache.GetAllDocuments<Bill>(DocType)?.ToList() ?? new List<Bill>();

        // 3. Подтягиваем свежие данные с бэкенда
        try
        {
            var remote = await _restClient.GetAsync<IEnumerable<Bill>>("/api/bills");
            if (remote != null && remote.Any())
            {
                foreach (var bill in remote)
                {
                    LocalSqliteCache.SaveDocument(DocType, bill.Id, bill, isSynced: true);
                }
                return remote.ToList();
            }
        }
        catch { }

        return localItems;
    }

    public async Task<int> CountAsync(params Expression<Func<Bill, bool>>[] predicates)
    {
        return (await GetAsync(predicates)).Count();
    }

    public async Task CreateAsync(Bill model) => await SaveAsync(model);

    public async Task UpdateAsync(Bill model) => await SaveAsync(model);

    public async Task<Bill> SaveAsync(Bill entity)
    {
        if (entity == null) return null;
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();

        // Мгновенно сохраняем в локальный SQLite
        LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

        try
        {
            await _restClient.PostAsync("/api/bills", entity);
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BILL SYNC WARNING]: {ex.Message}");
        }

        return entity;
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        try
        {
            await _restClient.DeleteAsync($"/api/bills/{id}");
        }
        catch { }
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        var dict = new Dictionary<string, Dictionary<string, int>>();
        if (fields != null)
        {
            foreach (var f in fields) dict[f] = new Dictionary<string, int>();
        }

        try
        {
            var fieldsParam = fields != null && fields.Length > 0 ? string.Join(",", fields) : "Date";
            var apiResult = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/bills/facets?fields={fieldsParam}");
            if (apiResult != null)
            {
                foreach (var kvp in apiResult) dict[kvp.Key] = kvp.Value;
            }
        }
        catch { }

        return dict;
    }
}