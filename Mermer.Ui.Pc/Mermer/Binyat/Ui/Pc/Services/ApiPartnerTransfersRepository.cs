using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.CRM.Models;
using Mermer.Data.Storage;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services
{
    public class ApiPartnerTransfersRepository : IRepository<PartnerTransfer>, IReadOnlyRepository<PartnerTransfer>, IRepositoryWithFacets<PartnerTransfer>
    {
        private readonly RestClient _restClient;
        private const string DocType = "PartnerTransfer";

        public ApiPartnerTransfersRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<PartnerTransfer>> GetAllAsync()
        {
            var local = LocalSqliteCache.GetAllDocuments<PartnerTransfer>(DocType).ToList();

            _ = Task.Run(async () =>
            {
                try
                {
                    // Отправляем несинхронизированные локальные документы на сервер
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<PartnerTransfer>(DocType);
                    foreach (var (id, transfer) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/partners/transfers", transfer);
                            LocalSqliteCache.SaveDocument(DocType, id, transfer, isSynced: true);
                        }
                        catch { }
                    }

                    // Скачиваем данные с сервера
                    var remote = await _restClient.GetAsync<List<PartnerTransfer>>("/api/partners/transfers");
                    if (remote != null)
                    {
                        foreach (var transfer in remote)
                        {
                            LocalSqliteCache.SaveDocument(DocType, transfer.Id, transfer, isSynced: true);
                        }
                    }
                }
                catch { }
            });

            return local;
        }

        public async Task<PartnerTransfer> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<PartnerTransfer>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<PartnerTransfer>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            return all.Where(s => idSet.Contains(s.Id));
        }

        public async Task<IEnumerable<PartnerTransfer>> GetAsync(params Expression<Func<PartnerTransfer, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<PartnerTransfer, bool>>[] predicates) => (await GetAsync(predicates)).Count();

        public async Task SaveAsync(PartnerTransfer entity)
        {
            if (entity == null) return;
            if (string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString())
                entity.Id = Guid.NewGuid().ToString();

            // Сохраняем локально моментально
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            // В фоне отправляем на сервер
            _ = Task.Run(async () =>
            {
                try
                {
                    await _restClient.PostAsync("/api/partners/transfers", entity);
                    LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
                }
                catch { }
            });
        }

        public async Task CreateAsync(PartnerTransfer entity) => await SaveAsync(entity);
        public async Task UpdateAsync(PartnerTransfer entity) => await SaveAsync(entity);
        public Task DeleteAsync(string id) => Task.CompletedTask;

        // Именно этот метод требовался интерфейсом IRepositoryWithFacets, из-за которого падал каст
        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
        {
            var result = new Dictionary<string, Dictionary<string, int>>();
            if (fields != null)
            {
                foreach (var field in fields) result[field] = new Dictionary<string, int>();
            }
            return await Task.FromResult(result);
        }
    }
}