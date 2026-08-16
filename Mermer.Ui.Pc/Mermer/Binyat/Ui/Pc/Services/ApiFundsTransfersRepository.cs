using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Finance.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiFundsTransfersRepository : IRepositoryWithFacets<FundsTransfer>, IRepository<FundsTransfer>, IReadOnlyRepository<FundsTransfer>
{
    private readonly RestClient _restClient;
    private const string DocType = "FundsTransfer";

    public ApiFundsTransfersRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<FundsTransfer> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        var allLocal = LocalSqliteCache.GetAllDocuments<FundsTransfer>(DocType);
        var local = allLocal?.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        if (local != null) return local;

        try
        {
            var remote = await _restClient.GetAsync<FundsTransfer>($"/api/finance/transfers/{id}");
            if (remote != null)
            {
                LocalSqliteCache.SaveDocument(DocType, remote.Id, remote, isSynced: true);
                return remote;
            }
        }
        catch { }

        return null;
    }

    public async Task<IEnumerable<FundsTransfer>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<FundsTransfer>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(x => idSet.Contains(x.Id)).ToList();
    }

    public async Task<IEnumerable<FundsTransfer>> GetAsync(params Expression<Func<FundsTransfer, bool>>[] predicates)
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

    private async Task<IEnumerable<FundsTransfer>> GetAllAsync()
    {
        // 1. Досылаем на сервер всё, что висит с isSynced == false
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<FundsTransfer>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PostAsync("/api/finance/transfers", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        // 2. Загружаем данные из локального SQLite
        var localItems = LocalSqliteCache.GetAllDocuments<FundsTransfer>(DocType)?.ToList() ?? new List<FundsTransfer>();

        // 3. Затягиваем свежие данные с бэкенда
        try
        {
            var remote = await _restClient.GetAsync<IEnumerable<FundsTransfer>>("/api/finance/transfers");
            if (remote != null && remote.Any())
            {
                foreach (var item in remote)
                {
                    LocalSqliteCache.SaveDocument(DocType, item.Id, item, isSynced: true);
                }
                return remote.ToList();
            }
        }
        catch { }

        return localItems;
    }

    public async Task<int> CountAsync(params Expression<Func<FundsTransfer, bool>>[] predicates)
    {
        return (await GetAsync(predicates)).Count();
    }

    public async Task CreateAsync(FundsTransfer model)
    {
        await SaveAsync(model);
    }

    public async Task UpdateAsync(FundsTransfer model)
    {
        await SaveAsync(model);
    }

    private async Task SaveAsync(FundsTransfer model)
    {
        if (model == null) return;
        if (string.IsNullOrEmpty(model.Id)) model.Id = Guid.NewGuid().ToString();

        // 1. Мгновенно сохраняем в локальный SQLite
        LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: false);

        // 2. Отправляем в бэкенд PostgreSQL
        try
        {
            await _restClient.PostAsync("/api/finance/transfers", model);
            LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FUNDS TRANSFER SYNC ERROR]: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        try
        {
            await _restClient.DeleteAsync($"/api/finance/transfers/{id}");
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
            var apiResult = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/finance/transfers/facets?fields={fieldsParam}");
            if (apiResult != null)
            {
                foreach (var kvp in apiResult) dict[kvp.Key] = kvp.Value;
            }
        }
        catch { }

        return dict;
    }
}