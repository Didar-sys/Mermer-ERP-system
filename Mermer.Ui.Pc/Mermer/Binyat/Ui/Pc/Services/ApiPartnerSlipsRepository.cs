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
    public class ApiPartnerSlipsRepository : IRepository<PartnerSlip>, IReadOnlyRepository<PartnerSlip>, IRepositoryWithFacets<PartnerSlip>
    {
        private readonly RestClient _restClient;
        private const string DocType = "PartnerSlip";

        public ApiPartnerSlipsRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<PartnerSlip>> GetAllAsync()
        {
            var local = LocalSqliteCache.GetAllDocuments<PartnerSlip>(DocType).ToList();

            _ = Task.Run(async () =>
            {
                try
                {
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<PartnerSlip>(DocType);
                    foreach (var (id, slip) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/partners/slips", slip);
                            LocalSqliteCache.SaveDocument(DocType, id, slip, isSynced: true);
                        }
                        catch { }
                    }

                    var remote = await _restClient.GetAsync<List<PartnerSlip>>("/api/partners/slips");
                    if (remote != null)
                    {
                        foreach (var slip in remote)
                        {
                            LocalSqliteCache.SaveDocument(DocType, slip.Id, slip, isSynced: true);
                        }
                    }
                }
                catch { }
            });

            return local;
        }

        public async Task<PartnerSlip> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<PartnerSlip>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<PartnerSlip>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            return all.Where(s => idSet.Contains(s.Id));
        }

        public async Task<IEnumerable<PartnerSlip>> GetAsync(params Expression<Func<PartnerSlip, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<PartnerSlip, bool>>[] predicates) => (await GetAsync(predicates)).Count();

        public async Task SaveAsync(PartnerSlip entity)
        {
            if (entity == null) return;
            if (string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString())
                entity.Id = Guid.NewGuid().ToString();

            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _restClient.PostAsync("/api/partners/slips", entity);
                    LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
                }
                catch { }
            });
        }

        public async Task CreateAsync(PartnerSlip entity) => await SaveAsync(entity);
        public async Task UpdateAsync(PartnerSlip entity) => await SaveAsync(entity);
        public Task DeleteAsync(string id) => Task.CompletedTask;

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