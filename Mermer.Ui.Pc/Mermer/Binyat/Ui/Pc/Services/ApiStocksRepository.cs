using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;

namespace Mermer.Ui.Pc.Services;

public class ApiStocksRepository : IRepository<Stock>, IReadOnlyRepository<Stock>, IRepositoryWithFacets<Stock>, IStocksRepository
{
    private readonly RestClient _restClient;
    private const string DocType = "Stock";

    public ApiStocksRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<IEnumerable<Stock>> GetAllAsync()
    {
        // 1. Досылаем неотправленные товары в фоне
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Stock>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PutAsync($"/api/stocks/{item.id}", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        // 2. Отдаем локальный кэш
        var local = LocalSqliteCache.GetAllDocuments<Stock>(DocType) ?? new List<Stock>();

        // 3. Фоновое обновление с сервера
        _ = Task.Run(async () =>
        {
            try
            {
                var remote = await _restClient.GetAsync<List<Stock>>("/api/stocks");
                if (remote != null)
                {
                    foreach (var s in remote)
                    {
                        LocalSqliteCache.SaveDocument(DocType, s.Id, s, isSynced: true);
                    }
                }
            }
            catch { }
        });

        return local;
    }

    public async Task<Stock> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var all = await GetAllAsync();
        return all.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Stock>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<Stock>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(s => idSet.Contains(s.Id)).ToList();
    }

    public async Task<IEnumerable<Stock>> GetListAsync(params string[] ids) => await GetAsync(ids);

    public async Task<IEnumerable<Stock>> GetAsync(params Expression<Func<Stock, bool>>[] predicates)
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

    public async Task<int> CountAsync(params Expression<Func<Stock, bool>>[] predicates)
    {
        var result = await GetAsync(predicates);
        return result.Count();
    }

    public async Task SaveAsync(Stock entity)
    {
        if (entity == null) return;
        bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
        if (isNew) entity.Id = Guid.NewGuid().ToString();

        LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

        try
        {
            if (isNew) await _restClient.PostAsync("/api/stocks", entity);
            else await _restClient.PutAsync($"/api/stocks/{entity.Id}", entity);
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
        }
        catch { }
    }

    public async Task CreateAsync(Stock entity) => await SaveAsync(entity);
    public async Task UpdateAsync(Stock entity) => await SaveAsync(entity);

    public async Task DeleteAsync(string id)
    {
        try { await _restClient.DeleteAsync($"/api/stocks/{id}"); } catch { }
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

    // --- МЕТОДЫ СПЕЦИФИЧЕСКИЕ ДЛЯ ISTOCKSREPOSITORY ---

    public async Task<IEnumerable<StockInfo>> GetInfoAsync(params string[] stockIds)
    {
        if (stockIds == null || !stockIds.Any()) return Enumerable.Empty<StockInfo>();
        var stocks = await GetAsync(stockIds);

        return stocks.Select(stock => new StockInfo
        {
            Id = stock.Id,
            Code = stock.Code,
            Name = stock.Name,
            ShortName = stock.ShortName,
            Unit = stock.Unit,
            Price = stock.Price,
            CurrencyId = stock.CurrencyId,
            Type = stock.Type,
            Group = stock.Group,
            Tags = stock.Tags?.ToList(),
            Barcodes = stock.Barcodes?.ToList(),
            IsDisabled = stock.IsDisabled
        }).ToList();
    }

    public async Task<IEnumerable<StockInfo>> GetInfoAsync(string additionalPriceCurrencyId, string additionalPriceGroup)
    {
        var all = await GetAllAsync();

        return all.Select(stock => new StockInfo
        {
            Id = stock.Id,
            Code = stock.Code,
            Name = stock.Name,
            ShortName = stock.ShortName,
            Unit = stock.Unit,
            Price = stock.Price,
            CurrencyId = stock.CurrencyId,
            Type = stock.Type,
            Group = stock.Group,
            Tags = stock.Tags?.ToList(),
            Barcodes = stock.Barcodes?.ToList(),
            IsDisabled = stock.IsDisabled
        }).ToList();
    }

    public async Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems)
    {
        try
        {
            await _restClient.PostAsync("/api/stocks/merge", new
            {
                MainStockId = mainStockId,
                MergeStockIds = mergeStockIds,
                DisableMergedItems = disableMergedItems
            });
        }
        catch { }
    }
}