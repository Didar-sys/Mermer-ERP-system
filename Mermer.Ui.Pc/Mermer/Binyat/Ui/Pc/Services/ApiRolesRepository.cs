using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Authorization.Models;
using Mermer.Data.Storage;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiRolesRepository : IRepository<Role>
{
    private readonly RestClient _restClient;
    private const string DocType = "Role";

    public ApiRolesRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<Role> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        var allLocal = LocalSqliteCache.GetAllDocuments<Role>(DocType);
        var local = allLocal?.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));

        if (local != null) return NormalizeAuthorizations(local);

        try
        {
            var remote = await _restClient.GetAsync<Role>($"/api/roles/{id}");
            if (remote != null)
            {
                LocalSqliteCache.SaveDocument(DocType, remote.Id, remote, isSynced: true);
                return NormalizeAuthorizations(remote);
            }
        }
        catch { }

        return null;
    }

    public async Task<IEnumerable<Role>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<Role>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(r => idSet.Contains(r.Id)).ToList();
    }

    public async Task<IEnumerable<Role>> GetAsync(params Expression<Func<Role, bool>>[] predicates)
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

    private async Task<IEnumerable<Role>> GetAllAsync()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Role>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        var payload = CreatePayload(item.entity);
                        await _restClient.PutAsync($"/api/roles/{item.id}", payload);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        var localItems = LocalSqliteCache.GetAllDocuments<Role>(DocType)?.ToList() ?? new List<Role>();

        try
        {
            var remote = await _restClient.GetAsync<IEnumerable<Role>>("/api/roles");
            if (remote != null && remote.Any())
            {
                foreach (var role in remote)
                {
                    LocalSqliteCache.SaveDocument(DocType, role.Id, role, isSynced: true);
                }
                return remote.Select(NormalizeAuthorizations).ToList();
            }
        }
        catch { }

        return localItems.Select(NormalizeAuthorizations).ToList();
    }

    public async Task<int> CountAsync(params Expression<Func<Role, bool>>[] predicates)
    {
        return (await GetAsync(predicates)).Count();
    }

    public async Task CreateAsync(Role model) => await SaveAsync(model, isNew: true);

    public async Task UpdateAsync(Role model) => await SaveAsync(model, isNew: false);

    private async Task SaveAsync(Role model, bool isNew)
    {
        if (model == null) return;
        if (string.IsNullOrEmpty(model.Id)) model.Id = Guid.NewGuid().ToString();

        LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: false);

        try
        {
            var payload = CreatePayload(model);

            if (isNew)
            {
                await _restClient.PostAsync("/api/roles", payload);
            }
            else
            {
                await _restClient.PutAsync($"/api/roles/{model.Id}", payload);
            }

            LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ROLE SYNC WARNING]: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        try
        {
            await _restClient.DeleteAsync($"/api/roles/{id}");
        }
        catch { }
    }

    // Сохраняет старую логику Couchbase для совместимости биндингов XAML
    private Role NormalizeAuthorizations(Role role)
    {
        if (role?.Authorizations != null && role.Authorizations.Any())
        {
            role.Authorizations = role.Authorizations.ToDictionary(
                x => x.Key.First().ToString().ToUpper() + x.Key.Substring(1),
                x => x.Value);
        }
        return role;
    }

    private object CreatePayload(Role model)
    {
        return new
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            IsDisabled = model.IsDisabled,
            Authorizations = model.Authorizations?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, int>()
        };
    }
}