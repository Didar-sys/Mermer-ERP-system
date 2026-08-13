using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.FundsManagement.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services
{
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
            // 1. Берем локальные записи
            var localCurrencies = LocalSqliteCache.GetAllDocuments<Currency>(DocType)?.ToList() ?? new List<Currency>();

            // 2. Фоновое обновление с сервера
            _ = Task.Run(async () =>
            {
                try
                {
                    var remote = await _restClient.GetAsync<List<Currency>>("/api/currencies");
                    if (remote != null && remote.Any())
                    {
                        foreach (var curr in remote)
                        {
                            LocalSqliteCache.SaveDocument(DocType, curr.Id, curr, isSynced: true);
                        }
                    }
                }
                catch { }
            });

            // 3. Защита от пустого списка
            if (!localCurrencies.Any())
            {
                localCurrencies.Add(new Currency
                {
                    Id = "e580c087-ac00-4373-8639-000d89af4aaf",
                    Name = "US Dollar",
                    IsDefault = true
                });
            }

            // --- КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ: Гарантируем, что хоть одна валюта является Default ---
            if (!localCurrencies.Any(c => c.IsDefault))
            {
                localCurrencies.First().IsDefault = true;
            }
            // ---------------------------------------------------------------------------------

            // 4. Гарантия от NullReference внутри свойств валюты
            foreach (var c in localCurrencies)
            {
                InitializeCollections(c);
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
                        if (genType == typeof(IEnumerable<>) || genType == typeof(ICollection<>) || genType == typeof(IList<>))
                        {
                            if (prop.GetValue(obj) == null)
                            {
                                var listType = typeof(List<>).MakeGenericType(prop.PropertyType.GetGenericArguments()[0]);
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
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<Currency, bool>>[] predicates) => (await GetAsync(predicates)).Count();
        public Task SaveAsync(Currency entity) => Task.CompletedTask;
        public Task CreateAsync(Currency entity) => Task.CompletedTask;
        public Task UpdateAsync(Currency entity) => Task.CompletedTask;
        public Task DeleteAsync(string id) => Task.CompletedTask;
    }
}