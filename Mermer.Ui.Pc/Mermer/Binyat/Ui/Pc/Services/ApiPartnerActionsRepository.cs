using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mermer.CRM.Models;
using Mermer.CRM.Services;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services
{
    public class ApiPartnerActionsRepository : IPartnerActionsRepository
    {
        private readonly RestClient _restClient;

        public ApiPartnerActionsRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<int> CountAsync(DateTime? startDate, DateTime? endDate, string partnerId, params string[] officeIds)
        {
            var items = await GetAsync(startDate, endDate, partnerId, officeIds);
            return items.Count();
        }

        public async Task<IEnumerable<PartnerAction>> GetAsync(DateTime? startDate, DateTime? endDate, string partnerId, params string[] officeIds)
        {
            try
            {
                var queryParams = new List<string>();
                if (startDate.HasValue) queryParams.Add($"from={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
                if (endDate.HasValue) queryParams.Add($"till={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
                if (!string.IsNullOrEmpty(partnerId)) queryParams.Add($"partnerId={partnerId}");

                string url = "/api/partners/actions" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");

                var remote = await _restClient.GetAsync<List<PartnerAction>>(url);
                if (remote != null)
                {
                    return remote;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Partner Actions Error]: {ex.Message}");
            }

            return Enumerable.Empty<PartnerAction>();
        }

        public Task<Dictionary<string, PartnerActionInfo[]>> GetByPartnersAsync(string officeId, params string[] partners)
        {
            return Task.FromResult(new Dictionary<string, PartnerActionInfo[]>());
        }
    }
}