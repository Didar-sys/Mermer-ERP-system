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

        public async Task<IEnumerable<PartnerBalanceByTypeWithBalance>> GetByTypeAsync(DateTime dateFrom, DateTime dateTill, string partnerId, params string[] officeIds)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"from={dateFrom:yyyy-MM-ddTHH:mm:ss}",
                    $"till={dateTill:yyyy-MM-ddTHH:mm:ss}"
                };
                if (!string.IsNullOrEmpty(partnerId)) queryParams.Add($"partnerId={partnerId}");

                string url = "/api/partners/balances/by-type?" + string.Join("&", queryParams);

                var remote = await _restClient.GetAsync<List<PartnerBalanceByTypeWithBalance>>(url);
                if (remote != null)
                {
                    return remote;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Partner Balances Error]: {ex.Message}");
            }

            return Enumerable.Empty<PartnerBalanceByTypeWithBalance>();
        }

        public Task<PartnerBalanceResult> GetBalanceToDateAsync(string officeId, string partnerId, DateTime date, string excludeTransactionId = null)
        {
            return Task.FromResult(new PartnerBalanceResult { Balance = 0 });
        }

        public Task<PartnerBalanceAggregated> GetByTypeAggregatedAsync(string[] officeIds, DateTime dateFrom, DateTime dateTill)
        {
            return Task.FromResult(new PartnerBalanceAggregated());
        }
    }
}