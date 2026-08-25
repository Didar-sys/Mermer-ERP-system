using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Finance.DailyRegistery.Models;
using Mermer.Finance.DailyRegistery.Services;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiDailyFundsRegisteriesRepository :
    IRepositoryWithFacets<DailyFundsRegistery>,
    IRepository<DailyFundsRegistery>,
    IReadOnlyRepository<DailyFundsRegistery>,
    IDailyFundsRegisteriesRepository
{
    private readonly RestClient _restClient;
    private const string DocType = "DailyFundsRegistery";

    public ApiDailyFundsRegisteriesRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<DailyFundsRegistery> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        var allLocal = LocalSqliteCache.GetAllDocuments<DailyFundsRegistery>(DocType);
        var local = allLocal?.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        if (local != null) return local;

        try
        {
            var remote = await _restClient.GetAsync<DailyFundsRegistery>($"/api/finance/registeries/{id}");
            if (remote != null)
            {
                LocalSqliteCache.SaveDocument(DocType, remote.Id, remote, isSynced: true);
                return remote;
            }
        }
        catch { }

        return null;
    }

    public async Task<IEnumerable<DailyFundsRegistery>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<DailyFundsRegistery>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(x => idSet.Contains(x.Id)).ToList();
    }

    // Обычный GetAsync для базовых интерфейсов
    public async Task<IEnumerable<DailyFundsRegistery>> GetAsync(params Expression<Func<DailyFundsRegistery, bool>>[] predicates)
    {
        var all = await GetAllAsync();
        var query = all.AsQueryable();

        if (predicates != null && predicates.Any())
        {
            foreach (var p in predicates.Where(x => x != null)) query = query.Where(p);
        }

        return query.ToList();
    }

    // Специфичный метод для IDailyFundsRegisteriesRepository (возвращает DailyFundsRegisteryInfo)
    async Task<IEnumerable<DailyFundsRegisteryInfo>> IDailyFundsRegisteriesRepository.GetAsync(params Expression<Func<DailyFundsRegistery, bool>>[] predicates)
    {
        var items = await GetAsync(predicates);

        // Конвертируем в Info. Балансы (Computed) пока оставляем пустыми, они будут считаться на бэкенде.
        var infos = items.Select(x => new DailyFundsRegisteryInfo
        {
            Id = x.Id,
            Code = x.Code,
            Date = x.Date,
            DepositoryId = x.DepositoryId,
            IsCompleted = x.IsCompleted,
            IsDisabled = x.IsDisabled,
            UserName = x.UserName,
            Group = x.Group,
            Tags = x.Tags,
            Description = x.Description,
            Lines = x.Lines,
            CurrencyConvertions = x.CurrencyConvertions,
            Computed = null
        }).ToList();

        return infos;
    }

    private async Task<IEnumerable<DailyFundsRegistery>> GetAllAsync()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<DailyFundsRegistery>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PostAsync("/api/finance/registeries", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        var localItems = LocalSqliteCache.GetAllDocuments<DailyFundsRegistery>(DocType)?.ToList() ?? new List<DailyFundsRegistery>();

        try
        {
            var remote = await _restClient.GetAsync<IEnumerable<DailyFundsRegistery>>("/api/finance/registeries");
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

    public async Task<int> CountAsync(params Expression<Func<DailyFundsRegistery, bool>>[] predicates)
    {
        return (await GetAsync(predicates)).Count();
    }

    public async Task CreateAsync(DailyFundsRegistery model) => await SaveAsync(model);
    public async Task UpdateAsync(DailyFundsRegistery model) => await SaveAsync(model);

    public async Task SaveAsync(DailyFundsRegistery model)
    {
        if (model == null) return;

        bool isNew = string.IsNullOrEmpty(model.Id) || model.Id == Guid.Empty.ToString();
        if (isNew) model.Id = Guid.NewGuid().ToString();

        // 1. Локальный кэш
        LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: false);

        // 2. Серверная синхронизация
        try
        {
            if (isNew)
                await _restClient.PostAsync("/api/finance/registeries", model);
            else
                await _restClient.PutAsync($"/api/finance/registeries/{model.Id}", model);

            LocalSqliteCache.SaveDocument(DocType, model.Id, model, isSynced: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DAILY REGISTERY SYNC ERROR]: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        try { await _restClient.DeleteAsync($"/api/finance/registeries/{id}"); } catch { }
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        var dict = new Dictionary<string, Dictionary<string, int>>();
        if (fields != null) foreach (var f in fields) dict[f] = new Dictionary<string, int>();

        try
        {
            var fieldsParam = fields != null && fields.Length > 0 ? string.Join(",", fields) : "Date";
            var apiResult = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/finance/registeries/facets?fields={fieldsParam}");
            if (apiResult != null)
            {
                foreach (var kvp in apiResult) dict[kvp.Key] = kvp.Value;
            }
        }
        catch { }

        return dict;
    }
}