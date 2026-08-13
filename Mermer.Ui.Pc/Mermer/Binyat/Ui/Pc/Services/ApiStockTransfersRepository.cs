using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Warehousing.Models;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockTransfersRepository : IRepository<StockTransfer>, IReadOnlyRepository<StockTransfer>
    {
        private readonly RestClient _restClient;
        private const string DocType = "StockTransfer"; // Идентификатор коллекции в SQLite

        public ApiStockTransfersRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        // --- ЧТЕНИЕ (СНАЧАЛА КЭШ, ЗАТЕМ СИНХРОНИЗАЦИЯ) ---
        public async Task<IEnumerable<StockTransfer>> GetAllAsync()
        {
            var localTransfers = LocalSqliteCache.GetAllDocuments<StockTransfer>(DocType);

            _ = Task.Run(async () =>
            {
                try
                {
                    // 1. Досылаем несинхронизированные перемещения
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<StockTransfer>(DocType);
                    foreach (var (id, transfer) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/warehousing/transfers", transfer);
                            LocalSqliteCache.SaveDocument(DocType, id, transfer, isSynced: true);
                        }
                        catch { }
                    }

                    // 2. Скачиваем свежие с сервера
                    var remote = await _restClient.GetAsync<List<StockTransfer>>("/api/warehousing/transfers");
                    if (remote != null)
                    {
                        foreach (var transfer in remote)
                        {
                            LocalSqliteCache.SaveDocument(DocType, transfer.Id, transfer, isSynced: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[StockTransfers Sync Error]: {ex.Message}");
                }
            });

            return localTransfers;
        }

        public async Task<StockTransfer> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<StockTransfer>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<StockTransfer>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            return all.Where(t => idSet.Contains(t.Id));
        }

        public async Task<IEnumerable<StockTransfer>> GetAsync(params Expression<Func<StockTransfer, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<StockTransfer, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        // --- CUD ОПЕРАЦИИ (СОХРАНЕНИЕ В SQLITE + REST) ---
        public async Task SaveAsync(StockTransfer entity)
        {
            if (entity == null) return;

            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            try
            {
                if (isNew)
                    await _restClient.PostAsync("/api/warehousing/transfers", entity);
                else
                    await _restClient.PutAsync($"/api/warehousing/transfers/{entity.Id}", entity);

                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StockTransfer Save Error]: {ex.Message}");
            }
        }

        public async Task CreateAsync(StockTransfer entity) => await SaveAsync(entity);
        public async Task UpdateAsync(StockTransfer entity) => await SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try
            {
                await _restClient.DeleteAsync($"/api/warehousing/transfers/{id}");
            }
            catch { }
        }
    }
}