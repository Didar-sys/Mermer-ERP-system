using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiWarehousesRepository : IRepository<Warehouse>, IReadOnlyRepository<Warehouse>
{
    private readonly RestClient _restClient;
    private const string DocType = "Warehouse";

    public ApiWarehousesRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        // 1. Досылаем неотправленные склады
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Warehouse>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PutAsync($"/api/enterprise/warehouses/{item.id}", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        // 2. Отдаем локальный кэш
        var local = LocalSqliteCache.GetAllDocuments<Warehouse>(DocType) ?? new List<Warehouse>();

        // 3. Фоновое обновление с сервера
        _ = Task.Run(async () =>
        {
            try
            {
                var remote = await _restClient.GetAsync<List<Warehouse>>("/api/enterprise/warehouses");
                if (remote != null)
                {
                    foreach (var w in remote)
                    {
                        LocalSqliteCache.SaveDocument(DocType, w.Id, w, isSynced: true);
                    }
                }
            }
            catch { }
        });

        return local;
    }

    public async Task<Warehouse> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var all = await GetAllAsync();
        return all.FirstOrDefault(w => string.Equals(w.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Warehouse>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<Warehouse>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(w => idSet.Contains(w.Id)).ToList();
    }

    public async Task<IEnumerable<Warehouse>> GetAsync(params Expression<Func<Warehouse, bool>>[] predicates)
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

    public async Task<int> CountAsync(params Expression<Func<Warehouse, bool>>[] predicates)
    {
        var result = await GetAsync(predicates);
        return result.Count();
    }

    public async Task SaveAsync(Warehouse entity)
    {
        if (entity == null) return;
        bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
        if (isNew) entity.Id = Guid.NewGuid().ToString();

        LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

        try
        {
            if (isNew) await _restClient.PostAsync("/api/enterprise/warehouses", entity);
            else await _restClient.PutAsync($"/api/enterprise/warehouses/{entity.Id}", entity);
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
        }
        catch { }
    }

    public async Task CreateAsync(Warehouse entity) => await SaveAsync(entity);
    public async Task UpdateAsync(Warehouse entity) => await SaveAsync(entity);

    public async Task DeleteAsync(string id)
    {
        try { await _restClient.DeleteAsync($"/api/enterprise/warehouses/{id}"); } catch { }
    }
}