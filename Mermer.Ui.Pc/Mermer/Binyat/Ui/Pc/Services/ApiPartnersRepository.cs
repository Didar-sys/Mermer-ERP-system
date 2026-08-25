using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.CRM.Models;
using Mermer.Data.Storage;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiPartnersRepository : IRepository<Partner>, IReadOnlyRepository<Partner>, IRepositoryWithFacets<Partner>
{
    private readonly RestClient _restClient;
    private const string DocType = "Partner";

    private List<Partner>? _ramCache;
    private bool _isSyncing = false;

    public ApiPartnersRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<IEnumerable<Partner>> GetAllAsync()
    {
        if (_ramCache == null)
        {
            _ramCache = LocalSqliteCache.GetAllDocuments<Partner>(DocType) ?? new List<Partner>();
        }

        if (!_isSyncing)
        {
            _isSyncing = true;
            _ = Task.Run(async () =>
            {
                try
                {
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Partner>(DocType);
                    if (unsynced != null)
                    {
                        foreach (var item in unsynced)
                        {
                            await _restClient.PutAsync($"/api/partners/{item.id}", item.entity);
                            LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                        }
                    }

                    var remote = await _restClient.GetAsync<List<Partner>>("/api/partners");
                    if (remote != null)
                    {
                        foreach (var p in remote)
                        {
                            LocalSqliteCache.SaveDocument(DocType, p.Id, p, isSynced: true);
                        }
                        _ramCache = remote;
                    }
                }
                catch { }
                finally { _isSyncing = false; }
            });
        }

        return _ramCache;
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
        return all.Where(p => idSet.Contains(p.Id)).ToList();
    }

    public async Task<IEnumerable<Partner>> GetAsync(params Expression<Func<Partner, bool>>[] predicates)
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

    public async Task<int> CountAsync(params Expression<Func<Partner, bool>>[] predicates)
    {
        var result = await GetAsync(predicates);
        return result.Count();
    }

    public async Task SaveAsync(Partner entity)
    {
        if (entity == null) return;
        bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
        if (isNew) entity.Id = Guid.NewGuid().ToString();

        LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

        if (_ramCache != null)
        {
            var existing = _ramCache.FirstOrDefault(x => x.Id == entity.Id);
            if (existing != null) _ramCache.Remove(existing);
            _ramCache.Add(entity);
        }

        try
        {
            if (isNew) await _restClient.PostAsync("/api/partners", entity);
            else await _restClient.PutAsync($"/api/partners/{entity.Id}", entity);
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
        }
        catch { }
    }

    public async Task CreateAsync(Partner entity) => await SaveAsync(entity);
    public async Task UpdateAsync(Partner entity) => await SaveAsync(entity);

    public async Task DeleteAsync(string id)
    {
        if (_ramCache != null)
        {
            var existing = _ramCache.FirstOrDefault(x => x.Id == id);
            if (existing != null) _ramCache.Remove(existing);
        }
        try { await _restClient.DeleteAsync($"/api/partners/{id}"); } catch { }
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        var result = new Dictionary<string, Dictionary<string, int>>();
        if (fields != null)
        {
            foreach (var field in fields) result[field] = new Dictionary<string, int>();
        }

        try
        {
            var fieldsParam = fields != null && fields.Length > 0 ? string.Join(",", fields) : "";
            var apiResult = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/partners/facets?fields={fieldsParam}");
            if (apiResult != null)
            {
                foreach (var kvp in apiResult) result[kvp.Key] = kvp.Value;
            }
        }
        catch { }

        return result;
    }
}