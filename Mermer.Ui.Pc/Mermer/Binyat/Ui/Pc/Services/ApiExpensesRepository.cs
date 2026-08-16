using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Finance.Spending.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiExpensesRepository : IRepositoryWithFacets<Expense>, IRepository<Expense>, IReadOnlyRepository<Expense>
{
    private readonly RestClient _restClient;
    private const string DocType = "Expense";

    public ApiExpensesRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<Expense> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        var allLocal = LocalSqliteCache.GetAllDocuments<Expense>(DocType);
        var local = allLocal?.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        if (local != null) return local;

        try
        {
            var remote = await _restClient.GetAsync<Expense>($"/api/expenses/{id}");
            if (remote != null)
            {
                LocalSqliteCache.SaveDocument(DocType, remote.Id, remote, isSynced: true);
                return remote;
            }
        }
        catch { }

        return null;
    }

    public async Task<IEnumerable<Expense>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<Expense>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(e => idSet.Contains(e.Id)).ToList();
    }

    public async Task<IEnumerable<Expense>> GetAsync(params Expression<Func<Expense, bool>>[] predicates)
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

    private async Task<IEnumerable<Expense>> GetAllAsync()
    {
        // 1. Досылаем неотправленные расходы
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Expense>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PostAsync("/api/expenses", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        // 2. Отдаем локальный кэш
        var local = LocalSqliteCache.GetAllDocuments<Expense>(DocType)?.ToList() ?? new List<Expense>();

        // 3. Скачиваем свежие с бэкенда
        try
        {
            var remote = await _restClient.GetAsync<IEnumerable<Expense>>("/api/expenses");
            if (remote != null && remote.Any())
            {
                foreach (var exp in remote)
                {
                    LocalSqliteCache.SaveDocument(DocType, exp.Id, exp, isSynced: true);
                }
                return remote.ToList();
            }
        }
        catch { }

        return local;
    }

    public async Task<int> CountAsync(params Expression<Func<Expense, bool>>[] predicates)
    {
        return (await GetAsync(predicates)).Count();
    }

    public async Task CreateAsync(Expense model) => await SaveAsync(model);

    public async Task UpdateAsync(Expense model) => await SaveAsync(model);

    public async Task SaveAsync(Expense model)
    {
        if (model == null) return;
        if (string.IsNullOrEmpty(model.Id)) model.Id = Guid.NewGuid().ToString();

        LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: false);

        try
        {
            await _restClient.PostAsync("/api/expenses", model);
            LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EXPENSE SYNC ERROR]: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        try
        {
            await _restClient.DeleteAsync($"/api/expenses/{id}");
        }
        catch { }
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        var dict = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase)
        {
            ["TypeNames"] = new Dictionary<string, int>(),
            ["GroupNames"] = new Dictionary<string, int>(),
            ["TagNames"] = new Dictionary<string, int>()
        };

        if (fields != null)
        {
            foreach (var f in fields.Where(x => !dict.ContainsKey(x)))
            {
                dict[f] = new Dictionary<string, int>();
            }
        }

        try
        {
            var fieldsParam = fields != null && fields.Length > 0 ? string.Join(",", fields) : "";
            var apiResult = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/expenses/facets?fields={fieldsParam}");
            if (apiResult != null)
            {
                foreach (var kvp in apiResult) dict[kvp.Key] = kvp.Value;
            }
        }
        catch { }

        return dict;
    }
}