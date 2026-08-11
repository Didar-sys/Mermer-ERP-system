using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.StockManagement.Models;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Ui.Pc.DTOs;
using Mermer.Warehousing.Models;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockSlipsRepository : IRepository<StockSlip>, IReadOnlyRepository<StockSlip>
    {
        private readonly RestClient _restClient;
        private const string DocType = "StockSlip"; // Идентификатор для SQLite

        public ApiStockSlipsRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        // --- ЧТЕНИЕ (СНАЧАЛА КЭШ, ЗАТЕМ СИНХРОНИЗАЦИЯ) ---
        public async Task<IEnumerable<StockSlip>> GetAllAsync()
        {
            // 1. Моментальное чтение из SQLite
            var localSlips = LocalSqliteCache.GetAllDocuments<StockSlip>(DocType);

            // 2. Фоновая синхронизация
            _ = Task.Run(async () =>
            {
                try
                {
                    // Досылаем оффлайн-записи на сервер
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<StockSlip>(DocType);
                    foreach (var (id, slip) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/catalog/slips", slip);
                            LocalSqliteCache.SaveDocument(DocType, id, slip, isSynced: true);
                        }
                        catch { }
                    }

                    // Скачиваем свежие DTO и обновляем SQLite
                    var dtos = await _restClient.GetAsync<List<StockSlipDto>>("/api/catalog/slips");
                    if (dtos != null)
                    {
                        foreach (var dto in dtos)
                        {
                            var slip = new StockSlip
                            {
                                Id = dto.Id,
                                Code = dto.Code,
                                Date = dto.Date,
                                IsCompleted = dto.IsCompleted,
                                Description = dto.Description
                            };
                            LocalSqliteCache.SaveDocument(DocType, slip.Id, slip, isSynced: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[StockSlips Sync Error]: {ex.Message}");
                }
            });

            return localSlips;
        }

        public async Task<StockSlip> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<StockSlip>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<StockSlip>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            return all.Where(s => idSet.Contains(s.Id));
        }

        public async Task<IEnumerable<StockSlip>> GetAsync(params Expression<Func<StockSlip, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<StockSlip, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        // --- РЕАЛЬНАЯ ЗАПИСЬ (CUD) ---

        public async Task SaveAsync(StockSlip entity)
        {
            if (entity == null) return;

            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            // Сохраняем в кэш
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            try
            {
                if (isNew)
                    await _restClient.PostAsync("/api/catalog/slips", entity);
                else
                    await _restClient.PutAsync($"/api/catalog/slips/{entity.Id}", entity);

                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
            }
            catch { }
        }

        public async Task CreateAsync(StockSlip entity) => await SaveAsync(entity);
        public async Task UpdateAsync(StockSlip entity) => await SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try { await _restClient.DeleteAsync($"/api/catalog/slips/{id}"); } catch { }
        }
    }
}