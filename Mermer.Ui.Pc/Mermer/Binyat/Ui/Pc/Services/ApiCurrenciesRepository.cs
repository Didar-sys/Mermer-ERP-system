using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.FundsManagement.Models;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Ui.Pc.DTOs;

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
            var localCurrencies = LocalSqliteCache.GetAllDocuments<Currency>(DocType);

            _ = Task.Run(async () =>
            {
                try
                {
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Currency>(DocType);
                    foreach (var (id, currency) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/enterprise/currencies", currency);
                            LocalSqliteCache.SaveDocument(DocType, id, currency, isSynced: true);
                        }
                        catch { }
                    }

                    var dtos = await _restClient.GetAsync<List<CurrencyDto>>("/api/enterprise/currencies");
                    if (dtos != null)
                    {
                        foreach (var dto in dtos)
                        {
                            var currency = new Currency { Id = dto.Id, Name = dto.Name };
                            LocalSqliteCache.SaveDocument(DocType, currency.Id, currency, isSynced: true);
                        }
                    }
                }
                catch { }
            });

            return localCurrencies;
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

        public async Task<int> CountAsync(params Expression<Func<Currency, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public async Task SaveAsync(Currency entity)
        {
            if (entity == null) return;

            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            try
            {
                if (isNew) await _restClient.PostAsync("/api/enterprise/currencies", entity);
                else await _restClient.PutAsync($"/api/enterprise/currencies/{entity.Id}", entity);
                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
            }
            catch { }
        }

        public async Task CreateAsync(Currency entity) => await SaveAsync(entity);
        public async Task UpdateAsync(Currency entity) => await SaveAsync(entity);
        public async Task DeleteAsync(string id) { try { await _restClient.DeleteAsync($"/api/enterprise/currencies/{id}"); } catch { } }
    }
}