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

        public ApiInvoicesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        // --- IInvoicesRepository СРЕЗЫ ДАННЫХ (РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА) ---

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

        // --- БАЗОВЫЕ МЕТОДЫ ЧТЕНИЯ ---

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await GetAsync(Array.Empty<Expression<Func<Invoice, bool>>>());
        }

        public async Task<Invoice> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            try
            {
                return await _restClient.GetAsync<Invoice>($"/api/invoices/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Invoice>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Invoice>();
            var list = new List<Invoice>();
            foreach (var id in ids)
            {
                var item = await GetAsync(id);
                if (item != null) list.Add(item);
            }
            return list;
        }

        public async Task<IEnumerable<Invoice>> GetAsync(params Expression<Func<Invoice, bool>>[] predicates)
        {
            return Enumerable.Empty<Invoice>();
        }

        public async Task<int> CountAsync(params Expression<Func<Invoice, bool>>[] predicates)
        {
            return 0;
        }

        // --- CUD ОПЕРАЦИИ ---

        public async Task CreateAsync(Invoice entity)
        {
            if (entity == null) return;
            await _restClient.PostAsync("/api/invoices", entity);
        }

        public async Task UpdateAsync(Invoice entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.Id)) return;
            await _restClient.PutAsync($"/api/invoices/{entity.Id}", entity);
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            await _restClient.DeleteAsync($"/api/invoices/{id}");
        }

        public async Task SaveAsync(Invoice entity)
        {
            if (entity == null) return;
            if (string.IsNullOrEmpty(entity.Id) || entity.Id == Guid.Empty.ToString())
                await CreateAsync(entity);
            else
                await UpdateAsync(entity);
        }

        private class CountResponse
        {
            public int Count { get; set; }
        }
    }
}