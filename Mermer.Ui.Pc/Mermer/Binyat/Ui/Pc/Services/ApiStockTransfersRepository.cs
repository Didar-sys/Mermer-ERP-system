using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Warehousing.Models;

namespace Mermer.Ui.Pc.Services;

public class ApiStockTransfersRepository : IRepositoryWithFacets<StockTransfer>, IRepository<StockTransfer>, IReadOnlyRepository<StockTransfer>
{
    private readonly RestClient _restClient;
    private const string DocType = "StockTransfer";

    public ApiStockTransfersRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<StockTransfer> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        var allLocal = LocalSqliteCache.GetAllDocuments<StockTransfer>(DocType);
        var local = allLocal?.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        if (local != null) return local;

        try
        {
            var remote = await _restClient.GetAsync<StockTransfer>($"/api/warehousing/transfers/{id}");
            if (remote != null)
            {
                LocalSqliteCache.SaveDocument(DocType, remote.Id, remote, isSynced: true);
                return remote;
            }
        }
        catch { }

        return null;
    }

    public async Task<IEnumerable<StockTransfer>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<StockTransfer>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(x => idSet.Contains(x.Id)).ToList();
    }

    public async Task<IEnumerable<StockTransfer>> GetAsync(params Expression<Func<StockTransfer, bool>>[] predicates)
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

    private async Task<IEnumerable<StockTransfer>> GetAllAsync()
    {
        // 1. Досылаем неотправленные перемещения
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<StockTransfer>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PostAsync("/api/warehousing/transfers", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        // 2. Локальный кэш
        var localItems = LocalSqliteCache.GetAllDocuments<StockTransfer>(DocType)?.ToList() ?? new List<StockTransfer>();

        // 3. Запрос с сервера
        try
        {
            var remote = await _restClient.GetAsync<IEnumerable<StockTransfer>>("/api/warehousing/transfers");
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

    public async Task<int> CountAsync(params Expression<Func<StockTransfer, bool>>[] predicates)
    {
        return (await GetAsync(predicates)).Count();
    }

    public async Task CreateAsync(StockTransfer model) => await SaveAsync(model);

    public async Task UpdateAsync(StockTransfer model) => await SaveAsync(model);

    public async Task SaveAsync(StockTransfer model)
    {
        if (model == null) return;
        if (string.IsNullOrEmpty(model.Id)) model.Id = Guid.NewGuid().ToString();

        LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: false);

        try
        {
            await _restClient.PostAsync("/api/warehousing/transfers", model);
            LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STOCK TRANSFER SYNC ERROR]: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        try
        {
            await _restClient.DeleteAsync($"/api/warehousing/transfers/{id}");
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
            var apiResult = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/warehousing/transfers/facets?fields={fieldsParam}");
            if (apiResult != null)
            {
                foreach (var kvp in apiResult) dict[kvp.Key] = kvp.Value;
            }
        }
        catch { }

        return dict;
    }
}