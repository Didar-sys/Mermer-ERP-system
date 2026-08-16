using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiDepositoriesRepository : IRepository<Depository>, IReadOnlyRepository<Depository>, IRepositoryWithFacets<Depository>
{
    private readonly RestClient _restClient;
    private const string DocType = "Depository";

    public ApiDepositoriesRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<IEnumerable<Depository>> GetAllAsync()
    {
        // 1. Досылаем неотправленные кассы
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Depository>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PutAsync($"/api/depositories/{item.id}", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        // 2. Отдаем локальный кэш
        var local = LocalSqliteCache.GetAllDocuments<Depository>(DocType) ?? new List<Depository>();

        // 3. Обновляем в фоне с сервера
        _ = Task.Run(async () =>
        {
            try
            {
                var remote = await _restClient.GetAsync<List<Depository>>("/api/depositories");
                if (remote != null)
                {
                    foreach (var dep in remote)
                    {
                        LocalSqliteCache.SaveDocument(DocType, dep.Id, dep, isSynced: true);
                    }
                }
            }
            catch { }
        });

        return local;
    }

    public async Task<Depository> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var all = await GetAllAsync();
        return all.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Depository>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<Depository>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(d => idSet.Contains(d.Id)).ToList();
    }

    public async Task<IEnumerable<Depository>> GetAsync(params Expression<Func<Depository, bool>>[] predicates)
    {
        var all = await GetAllAsync();
        var query = all.AsQueryable();
        if (predicates != null)
        {
            foreach (var p in predicates.Where(x => x != null))
            {
                query = query.Where(p);
            }
        }
        return query.ToList();
    }

    public async Task<int> CountAsync(params Expression<Func<Depository, bool>>[] predicates)
    {
        var result = await GetAsync(predicates);
        return result.Count();
    }

    public async Task SaveAsync(Depository entity)
    {
        if (entity == null) return;
        bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
        if (isNew) entity.Id = Guid.NewGuid().ToString();

        LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

        try
        {
            if (isNew) await _restClient.PostAsync("/api/depositories", entity);
            else await _restClient.PutAsync($"/api/depositories/{entity.Id}", entity);
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
        }
        catch { }
    }

    public async Task CreateAsync(Depository entity) => await SaveAsync(entity);
    public async Task UpdateAsync(Depository entity) => await SaveAsync(entity);

    public async Task DeleteAsync(string id)
    {
        try { await _restClient.DeleteAsync($"/api/depositories/{id}"); } catch { }
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