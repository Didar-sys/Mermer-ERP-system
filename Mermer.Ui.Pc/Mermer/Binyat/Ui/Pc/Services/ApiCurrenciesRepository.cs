using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.FundsManagement.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiCurrenciesRepository : IRepository<Currency>, IReadOnlyRepository<Currency>
{
    private readonly RestClient _restClient;
    private const string DocType = "Currency";

    public ApiCurrenciesRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<IEnumerable<Currency>> GetAllAsync()
    {
        // 1. Досылаем на сервер всё, что не синхронизировано
        _ = Task.Run(async () =>
        {
            try
            {
                var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Currency>(DocType);
                if (unsynced != null)
                {
                    foreach (var item in unsynced)
                    {
                        await _restClient.PutAsync($"/api/currencies/{item.id}", item.entity);
                        LocalSqliteCache.SaveDocument(DocType, item.id, item.entity, isSynced: true);
                    }
                }
            }
            catch { }
        });

        // 2. Фоновый запрос актуальных курсов с сервера
        try
        {
            var remote = await _restClient.GetAsync<List<CurrencyDto>>("/api/currencies");
            if (remote != null && remote.Any())
            {
                var resultList = new List<Currency>();

                foreach (var dto in remote)
                {
                    var curr = new Currency
                    {
                        Id = dto.Id,
                        Name = dto.Name,
                        Decimals = dto.Decimals,
                        IsDefault = dto.IsDefault,
                        IsDisabled = dto.IsDisabled,
                        Description = dto.Description,
                        Rates = new ObservableCollection<CurrencyRate>()
                    };

                    if (dto.Rates != null && dto.Rates.Any())
                    {
                        foreach (var rDto in dto.Rates.OrderBy(r => r.ValidFrom))
                        {
                            curr.Rates.Add(new CurrencyRate
                            {
                                Id = rDto.Id ?? Guid.NewGuid().ToString(),
                                ValidFrom = new DateTime(rDto.ValidFrom.Year, rDto.ValidFrom.Month, rDto.ValidFrom.Day, 0, 0, 0, DateTimeKind.Local),
                                Multiplier = rDto.Multiplier != 0 ? rDto.Multiplier : 1m,
                                Divider = rDto.Divider != 0 ? rDto.Divider : 1m
                            });
                        }

                        var oldestRate = curr.Rates.First();
                        curr.Rates.Add(new CurrencyRate
                        {
                            Id = Guid.NewGuid().ToString(),
                            ValidFrom = DateTime.MinValue,
                            Multiplier = oldestRate.Multiplier,
                            Divider = oldestRate.Divider
                        });
                    }
                    else
                    {
                        curr.Rates.Add(new CurrencyRate { ValidFrom = DateTime.MinValue, Multiplier = 1m, Divider = 1m });
                    }

                    InitializeCollections(curr);
                    LocalSqliteCache.SaveDocument(DocType, curr.Id, curr, isSynced: true);
                    resultList.Add(curr);
                }

                return resultList;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CURRENCY FETCH ERROR]: {ex.Message}");
        }

        var localCurrencies = LocalSqliteCache.GetAllDocuments<Currency>(DocType)?.ToList() ?? new List<Currency>();

        if (!localCurrencies.Any())
        {
            var defaultUsd = new Currency
            {
                Id = "e580c087-ac00-4373-8639-000d89af4aaf",
                Name = "USD",
                IsDefault = true,
                Rates = new ObservableCollection<CurrencyRate>
                {
                    new CurrencyRate { ValidFrom = DateTime.MinValue, Multiplier = 1m, Divider = 1m }
                }
            };
            localCurrencies.Add(defaultUsd);
        }

        if (!localCurrencies.Any(c => c.IsDefault))
        {
            localCurrencies.First().IsDefault = true;
        }

        foreach (var c in localCurrencies)
        {
            InitializeCollections(c);
            if (c.Rates == null || !c.Rates.Any())
            {
                c.Rates = new ObservableCollection<CurrencyRate>
                {
                    new CurrencyRate { ValidFrom = DateTime.MinValue, Multiplier = 1m, Divider = 1m }
                };
            }
        }

        return localCurrencies;
    }

    private void InitializeCollections(object obj)
    {
        if (obj == null) return;
        try
        {
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanWrite && prop.PropertyType.IsGenericType)
                {
                    var genType = prop.PropertyType.GetGenericTypeDefinition();
                    if (genType == typeof(IEnumerable<>) || genType == typeof(ICollection<>) || genType == typeof(IList<>) || genType == typeof(ObservableCollection<>))
                    {
                        if (prop.GetValue(obj) == null)
                        {
                            var listType = typeof(ObservableCollection<>).MakeGenericType(prop.PropertyType.GetGenericArguments()[0]);
                            prop.SetValue(obj, Activator.CreateInstance(listType));
                        }
                    }
                }
            }
        }
        catch { }
    }

    public async Task<Currency> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var all = await GetAllAsync();
        return all.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Currency>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<Currency>();
        var all = await GetAllAsync();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(c => idSet.Contains(c.Id));
    }

    public async Task<IEnumerable<Currency>> GetAsync(params Expression<Func<Currency, bool>>[] predicates)
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

    public async Task<int> CountAsync(params Expression<Func<Currency, bool>>[] predicates) => (await GetAsync(predicates)).Count();

    public async Task CreateAsync(Currency entity) => await SaveAsync(entity);
    public async Task UpdateAsync(Currency entity) => await SaveAsync(entity);

    public async Task SaveAsync(Currency entity)
    {
        if (entity == null) return;
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();

        LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

        try
        {
            await _restClient.PutAsync($"/api/currencies/{entity.Id}", entity);
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CURRENCY SAVE ERROR]: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        try
        {
            await _restClient.DeleteAsync($"/api/currencies/{id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CURRENCY DELETE ERROR]: {ex.Message}");
        }
    }

    private class CurrencyDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Decimals { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDisabled { get; set; }
        public string Description { get; set; }
        public List<CurrencyRateDto> Rates { get; set; }
    }

    private class CurrencyRateDto
    {
        public string Id { get; set; }
        public DateTime ValidFrom { get; set; }
        public decimal Multiplier { get; set; }
        public decimal Divider { get; set; }
    }
}