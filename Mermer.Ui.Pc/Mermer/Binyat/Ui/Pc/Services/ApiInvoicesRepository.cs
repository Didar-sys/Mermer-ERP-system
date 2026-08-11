using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Commerce.Models;
using Mermer.Commerce.Services;
using Mermer.Data.Storage;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services
{
    public class ApiInvoicesRepository : IRepository<Invoice>, IReadOnlyRepository<Invoice>, IInvoicesRepository
    {
        private readonly RestClient _restClient;
        private const string DocType = "Invoice";

        public ApiInvoicesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        // --- IInvoicesRepository СРЕЗЫ ДАННЫХ ---

        public Task<IEnumerable<InvoiceInfo>> GetInfoAsync(DateTime from, DateTime till)
        {
            return GetInfoAsync(from, till, null);
        }

        public async Task<IEnumerable<InvoiceInfo>> GetInfoAsync(DateTime from, DateTime till, string displayCurrencyId)
        {
            try
            {
                var fromStr = from.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var tillStr = till.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var url = $"/api/invoices?from={fromStr}&till={tillStr}";
                if (!string.IsNullOrEmpty(displayCurrencyId)) url += $"&displayCurrencyId={displayCurrencyId}";

                var result = await _restClient.GetAsync<List<InvoiceInfo>>(url);
                return result ?? Enumerable.Empty<InvoiceInfo>();
            }
            catch
            {
                return Enumerable.Empty<InvoiceInfo>();
            }
        }

        public async Task<int> CountInfoAsync(DateTime from, DateTime till)
        {
            try
            {
                var fromStr = from.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var tillStr = till.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var res = await _restClient.GetAsync<CountResponse>($"/api/invoices/count?from={fromStr}&till={tillStr}");
                return res?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public Task<IEnumerable<InvoicePaymentInfo>> GetPaymentInfoAsync(DateTime from, DateTime till, string officeId, string partnerId)
        {
            return GetPaymentInfoAsync(from, till, officeId, partnerId, null);
        }

        public async Task<IEnumerable<InvoicePaymentInfo>> GetPaymentInfoAsync(DateTime from, DateTime till, string officeId, string partnerId, string displayCurrencyId)
        {
            try
            {
                var fromStr = from.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var tillStr = till.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var url = $"/api/invoices/payment-info?from={fromStr}&till={tillStr}";
                if (!string.IsNullOrEmpty(officeId)) url += $"&officeId={officeId}";
                if (!string.IsNullOrEmpty(partnerId)) url += $"&partnerId={partnerId}";
                if (!string.IsNullOrEmpty(displayCurrencyId)) url += $"&displayCurrencyId={displayCurrencyId}";

                var result = await _restClient.GetAsync<List<InvoicePaymentInfo>>(url);
                return result ?? Enumerable.Empty<InvoicePaymentInfo>();
            }
            catch
            {
                return Enumerable.Empty<InvoicePaymentInfo>();
            }
        }

        public async Task<int> CountPaymentInfoAsync(DateTime from, DateTime till, string officeId = null, string partnerId = null)
        {
            try
            {
                var fromStr = from.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var tillStr = till.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var url = $"/api/invoices/payment-info/count?from={fromStr}&till={tillStr}";
                if (!string.IsNullOrEmpty(officeId)) url += $"&officeId={officeId}";
                if (!string.IsNullOrEmpty(partnerId)) url += $"&partnerId={partnerId}";

                var res = await _restClient.GetAsync<CountResponse>(url);
                return res?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        // --- БАЗОВЫЕ МЕТОДЫ ЧТЕНИЯ (ГИБРИДНЫЕ: SQLITE + API) ---

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            var local = LocalSqliteCache.GetAllDocuments<Invoice>(DocType);

            _ = Task.Run(async () =>
            {
                try
                {
                    // 1. Досылаем несинхронизированные накладные
                    var unsynced = LocalSqliteCache.GetUnsyncedDocuments<Invoice>(DocType);
                    foreach (var (id, inv) in unsynced)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/invoices", inv);
                            LocalSqliteCache.SaveDocument(DocType, id, inv, isSynced: true);
                        }
                        catch { }
                    }

                    // 2. Скачиваем свежие с сервера
                    var remote = await _restClient.GetAsync<List<Invoice>>("/api/invoices");
                    if (remote != null)
                    {
                        foreach (var inv in remote)
                        {
                            LocalSqliteCache.SaveDocument(DocType, inv.Id, inv, isSynced: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Invoice Sync Error]: {ex.Message}");
                }
            });

            return local;
        }

        public async Task<Invoice> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(i => string.Equals(i.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Invoice>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Invoice>();
            var all = await GetAllAsync();
            var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            return all.Where(i => idSet.Contains(i.Id));
        }

        public async Task<IEnumerable<Invoice>> GetAsync(params Expression<Func<Invoice, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<Invoice, bool>>[] predicates)
        {
            var result = await GetAsync(predicates);
            return result.Count();
        }

        // --- CUD ОПЕРАЦИИ (СОХРАНЕНИЕ В SQLITE + POST/PUT) ---

        public async Task SaveAsync(Invoice entity)
        {
            if (entity == null) return;

            bool isNew = string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString();
            if (isNew) entity.Id = Guid.NewGuid().ToString();

            // Сохраняем локально
            LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: false);

            try
            {
                if (isNew)
                    await _restClient.PostAsync("/api/invoices", entity);
                else
                    await _restClient.PutAsync($"/api/invoices/{entity.Id}", entity);

                LocalSqliteCache.SaveDocument(DocType, entity.Id, entity, isSynced: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Invoice Save Error]: {ex.Message}");
            }
        }

        public async Task CreateAsync(Invoice entity) => await SaveAsync(entity);
        public async Task UpdateAsync(Invoice entity) => await SaveAsync(entity);

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try
            {
                await _restClient.DeleteAsync($"/api/invoices/{id}");
            }
            catch { }
        }

        private class CountResponse
        {
            public int Count { get; set; }
        }
    }
}