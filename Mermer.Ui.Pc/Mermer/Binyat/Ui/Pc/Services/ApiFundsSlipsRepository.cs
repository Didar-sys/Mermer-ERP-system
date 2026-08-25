using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Finance.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiFundsSlipsRepository : IRepositoryWithFacets<FundsSlip>, IRepository<FundsSlip>, IReadOnlyRepository<FundsSlip>
{
    private readonly RestClient _restClient;
    private const string DocType = "FundsSlip";

    public ApiFundsSlipsRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<FundsSlip> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        var allLocal = LocalSqliteCache.GetAllDocuments<FundsSlip>(DocType);
        var local = allLocal?.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        if (local != null) return local;

        try
        {
            var remote = await _restClient.GetAsync<FundsSlip>($"/api/finance/slips/{id}");
            if (remote != null)
            {
                LocalSqliteCache.SaveDocument(DocType, remote.Id, remote, isSynced: true);
                return remote;
            }
        }
        catch { }

        return null;
    }

    public async Task<IEnumerable<FundsSlip>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<FundsSlip>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(x => idSet.Contains(x.Id)).ToList();
    }

    public async Task<IEnumerable<FundsSlip>> GetAsync(params Expression<Func<FundsSlip, bool>>[] predicates)
    {
        var all = await GetAllAsync();
        var query = all.AsQueryable();

        if (predicates != null && predicates.Any())
        {
            foreach (var p in predicates.Where(x => x != null))
            {
                query = query.Where(p);
            }
        }

        return query.ToList();
    }

    private async Task<IEnumerable<FundsSlip>> GetAllAsync()
    {
        // 1. Досылаем на сервер всё, что не синхронизировано
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<FundsSlip>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PostAsync("/api/finance/slips", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FundsSlips Sync Error]: {ex.Message}");
            }
        });

        // 2. Локальный кэш
        var localItems = LocalSqliteCache.GetAllDocuments<FundsSlip>(DocType)?.ToList() ?? new List<FundsSlip>();

        // 3. Запрос с сервера
        try
        {
            var remote = await _restClient.GetAsync<IEnumerable<FundsSlip>>("/api/finance/slips");
            if (remote != null && remote.Any())
            {
                foreach (var slip in remote)
                {
                    LocalSqliteCache.SaveDocument(DocType, slip.Id, slip, isSynced: true);
                }
                return remote.ToList();
            }
        }
        catch { }

        return localItems;
    }

    public async Task<int> CountAsync(params Expression<Func<FundsSlip, bool>>[] predicates)
    {
        return (await GetAsync(predicates)).Count();
    }

    public async Task CreateAsync(FundsSlip model) => await SaveAsync(model);

    public async Task UpdateAsync(FundsSlip model) => await SaveAsync(model);

    private async Task SaveAsync(FundsSlip model)
    {
        if (model == null) return;
        if (string.IsNullOrEmpty(model.Id)) model.Id = Guid.NewGuid().ToString();

        // 1. Мгновенная запись в локальный SQLite
        LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: false);

        // 2. Синхронизация с сервером
        try
        {
            await _restClient.PostAsync("/api/finance/slips", model);
            LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FUNDS SLIP SYNC ERROR]: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        try
        {
            await _restClient.DeleteAsync($"/api/finance/slips/{id}");
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
            var apiResult = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/finance/slips/facets?fields={fieldsParam}");
            if (apiResult != null)
            {
                foreach (var kvp in apiResult) dict[kvp.Key] = kvp.Value;
            }
        }
        catch { }

        return dict;
    }
}