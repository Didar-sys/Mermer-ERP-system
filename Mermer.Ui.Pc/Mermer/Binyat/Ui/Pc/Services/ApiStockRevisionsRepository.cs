using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.StockManagement.Models;
using Mermer.Warehousing.Revisioning.Models;
using Mermer.Warehousing.Revisioning.Services;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockRevisionsRepository :
        IStockRevisionsRepository,
        IRepository<StockRevision>,
        IReadOnlyRepository<StockRevision>,
        IRepositoryWithFacets<StockRevision>
    {
        private readonly RestClient _restClient;
        private const string DocType = "StockRevision";

        public ApiStockRevisionsRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<StockRevision>> GetAllAsync()
        {
            var local = LocalSqliteCache.GetAllDocuments<StockRevision>(DocType)?.ToList() ?? new List<StockRevision>();
            if (!local.Any())
            {
                try
                {
                    var remote = await _restClient.GetAsync<List<StockRevision>>("/api/warehousing/revisions");
                    if (remote != null && remote.Any())
                    {
                        foreach (var r in remote) LocalSqliteCache.SaveDocument(DocType, r.Id, r, true);
                        return remote;
                    }
                }
                catch { return local; }
            }
            return local;
        }

        public async Task<StockRevision> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(x => x.Id == id);
        }

        public async Task<IEnumerable<StockRevision>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<StockRevision>();
            var all = await GetAllAsync();
            return all.Where(x => ids.Contains(x.Id)).ToList();
        }

        public async Task<IEnumerable<StockRevision>> GetAsync(params Expression<Func<StockRevision, bool>>[] predicates)
        {
            var query = (await GetAllAsync()).AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates.Where(x => x != null)) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<StockRevision, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        public async Task SaveAsync(StockRevision entity)
        {
            if (entity == null) return;
            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, false);
            try
            {
                if (isNew)
                    await _restClient.PostAsync("/api/warehousing/revisions", entity);
                else
                    await _restClient.PutAsync($"/api/warehousing/revisions/{entity.Id}", entity);

                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, true);
            }
            catch { }
        }

        public Task CreateAsync(StockRevision entity) => SaveAsync(entity);
        public Task UpdateAsync(StockRevision entity) => SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try { await _restClient.DeleteAsync($"/api/warehousing/revisions/{id}"); } catch { }
        }

        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
        {
            var dict = new Dictionary<string, Dictionary<string, int>>();
            if (fields != null) foreach (var f in fields) dict[f] = new Dictionary<string, int>();

            try
            {
                var fieldsParam = fields != null && fields.Length > 0 ? string.Join(",", fields) : "Date";
                var apiResult = await _restClient.GetAsync<Dictionary<string, Dictionary<string, int>>>($"/api/warehousing/revisions/facets?fields={fieldsParam}");
                if (apiResult != null)
                {
                    foreach (var kvp in apiResult) dict[kvp.Key] = kvp.Value;
                }
            }
            catch { }

            return dict;
        }

        public async Task<IEnumerable<StockRevisionLine>> GetLinesAsync(string revisionId, params string[] lineIds)
        {
            if (string.IsNullOrEmpty(revisionId)) return Enumerable.Empty<StockRevisionLine>();
            try
            {
                var remote = await _restClient.GetAsync<List<StockRevisionLine>>($"/api/warehousing/revisions/{revisionId}/lines");
                if (remote != null && lineIds != null && lineIds.Any())
                    return remote.Where(l => lineIds.Contains(l.Id)).ToList();
                return remote ?? Enumerable.Empty<StockRevisionLine>();
            }
            catch { return Enumerable.Empty<StockRevisionLine>(); }
        }

        public async Task<StockRevisionLine> GetLineAsync(string stockRevisionLineId)
        {
            if (string.IsNullOrEmpty(stockRevisionLineId)) return null;
            return (await GetLinesAsync(null, stockRevisionLineId)).FirstOrDefault();
        }

        public async Task StoreLineAsync(StockRevisionLine line)
        {
            if (line == null || string.IsNullOrEmpty(line.StockRevisionId)) return;
            try
            {
                await _restClient.PostAsync($"/api/warehousing/revisions/{line.StockRevisionId}/lines", line);
            }
            catch { }
        }

        public async Task StoreLinesAsync(string revisionId, IEnumerable<StockRevisionLine> list)
        {
            if (string.IsNullOrEmpty(revisionId) || list == null) return;
            foreach (var line in list)
            {
                line.StockRevisionId = revisionId;
                await StoreLineAsync(line);
            }
        }

        public async Task DeleteLineAsync(string stockRevisionLineId)
        {
            if (string.IsNullOrEmpty(stockRevisionLineId)) return;
            try
            {
                await _restClient.DeleteAsync($"/api/warehousing/revisions/lines/{stockRevisionLineId}");
            }
            catch { }
        }

        public async Task<IEnumerable<StockRevisionLineInfo>> CalcLineInfosAsync(
            StockRevision revision,
            IEnumerable<StockRevisionLine> lines,
            Func<string[], Task<IEnumerable<Stock>>> stocksGetter,
            Func<string[], Task<IEnumerable<StockBalance>>> stockBalancesGetter,
            Func<(string stockId, DateTime? balanceDate)[], Task<IEnumerable<StockBalance>>> stockBalancesGetterAlt,
            string priceDisplayCurrencyId = null)
        {
            var revLines = lines?.ToArray() ?? Array.Empty<StockRevisionLine>();
            if (!revLines.Any()) return Enumerable.Empty<StockRevisionLineInfo>();

            var stockIds = revLines.Select(x => x.StockId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
            var stocks = (await stocksGetter(stockIds)).ToDictionary(x => x.Id, x => x);
            var balances = (await stockBalancesGetter(stockIds)).GroupBy(b => b.StockId).ToDictionary(g => g.Key, g => g.Sum(x => x.Balance));

            return revLines.Select(l =>
            {
                stocks.TryGetValue(l.StockId ?? "", out var stock);
                balances.TryGetValue(l.StockId ?? "", out var computed);

                decimal price = l.Price ?? stock?.Price ?? 0m;
                string unitName = stock?.Units?.FirstOrDefault(u => u.Id == l.UnitId)?.Name ?? stock?.Unit ?? "";

                return new StockRevisionLineInfo
                {
                    StockRevisionId = l.StockRevisionId,
                    StockRevisionLineId = l.Id,
                    StockId = l.StockId,
                    StockCode = stock?.Code ?? "",
                    StockName = stock?.Name ?? "",
                    StockPrice = price,
                    StockPriceCurrencyId = l.CurrencyId ?? stock?.CurrencyId ?? "",
                    Date = l.Date,
                    Quantity = l.Quantity,
                    UnitId = l.UnitId,
                    Unit = unitName,
                    TotalCounted = l.Quantity,
                    TotalComputed = computed,
                    UserId = l.UserId,
                    UserName = l.UserName
                };
            }).ToList();
        }

        public Task<IEnumerable<StockRevisionLineInfo>> GetLineInfosAsync(string revisionId, params string[] lineIds) => Task.FromResult(Enumerable.Empty<StockRevisionLineInfo>());
        public Task<StockRevisionCountInfo> GetCountInfoAsync(string revisionId, string stockId, Func<string, DateTime?> countDateGetter = null) => Task.FromResult(new StockRevisionCountInfo { StockId = stockId });
        public Task<IEnumerable<StockRevisionCountInfoWithData>> GetCountInfosAsync(string revisionId, string priceDisplayCurrencyId = null) => Task.FromResult(Enumerable.Empty<StockRevisionCountInfoWithData>());
        public Task<IEnumerable<StockRevisionUncountedInfo>> GetUncountedAsync(string revisionId) => Task.FromResult(Enumerable.Empty<StockRevisionUncountedInfo>());
    }
}