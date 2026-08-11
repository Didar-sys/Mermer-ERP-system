using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mermer.CRM.Models;
using Mermer.CRM.Services;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services
{
    public class ApiPartnerBalancesRepository : IPartnerBalancesRepository
    {
        private readonly RestClient _restClient;

        public ApiPartnerBalancesRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<PartnerBalanceResult> GetBalanceToDateAsync(
            string officeId,
            string partnerId,
            DateTime date,
            string excludeTransactionId = null)
        {
            try
            {
                var dateStr = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var url = $"/api/partners/balances/date?partnerId={partnerId}&date={dateStr}";
                if (!string.IsNullOrEmpty(officeId)) url += $"&officeId={officeId}";
                if (!string.IsNullOrEmpty(excludeTransactionId)) url += $"&excludeTransactionId={excludeTransactionId}";

                var res = await _restClient.GetAsync<PartnerBalanceResult>(url);
                return res ?? new PartnerBalanceResult { Balance = 0 };
            }
            catch
            {
                return new PartnerBalanceResult { Balance = 0 };
            }
        }

        public async Task<IEnumerable<PartnerBalanceByTypeWithBalance>> GetByTypeAsync(
            DateTime dateFrom,
            DateTime dateTill,
            string partnerId,
            params string[] officeIds)
        {
            try
            {
                var fromStr = dateFrom.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var tillStr = dateTill.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var url = $"/api/partners/balances/by-type?partnerId={partnerId}&from={fromStr}&till={tillStr}";

                var res = await _restClient.GetAsync<List<PartnerBalanceByTypeWithBalance>>(url);
                return res ?? Enumerable.Empty<PartnerBalanceByTypeWithBalance>();
            }
            catch
            {
                return Enumerable.Empty<PartnerBalanceByTypeWithBalance>();
            }
        }

        public async Task<PartnerBalanceAggregated> GetByTypeAggregatedAsync(
            string[] officeIds,
            DateTime dateFrom,
            DateTime dateTill)
        {
            try
            {
                var fromStr = dateFrom.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var tillStr = dateTill.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var url = $"/api/partners/balances/aggregated?from={fromStr}&till={tillStr}";

                var res = await _restClient.GetAsync<PartnerBalanceAggregated>(url);
                return res ?? new PartnerBalanceAggregated();
            }
            catch
            {
                return new PartnerBalanceAggregated();
            }
        }
    }
}