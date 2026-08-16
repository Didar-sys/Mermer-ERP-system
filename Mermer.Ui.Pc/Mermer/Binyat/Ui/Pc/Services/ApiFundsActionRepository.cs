using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Services;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiFundsActionRepository : IFundsActionsRepository
{
    private readonly RestClient _restClient;

    public ApiFundsActionRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<int> CountAsync(DateTime? startDate, DateTime? endDate, string currencyId, params string[] depositoryIds)
    {
        var list = await GetAsync(startDate, endDate, currencyId, depositoryIds);
        return list.Count();
    }

    public async Task<IEnumerable<FundsAction>> GetAsync(DateTime? startDate, DateTime? endDate, string currencyId, params string[] depositoryIds)
    {
        try
        {
            var queryParams = new List<string>();

            if (startDate.HasValue) queryParams.Add($"from={startDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            if (endDate.HasValue) queryParams.Add($"till={endDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            if (!string.IsNullOrEmpty(currencyId)) queryParams.Add($"currencyId={currencyId}");

            if (depositoryIds != null && depositoryIds.Any())
            {
                foreach (var depId in depositoryIds.Where(d => !string.IsNullOrEmpty(d)))
                {
                    queryParams.Add($"depositoryId={depId}");
                }
            }

            string url = "/api/finance/actions" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");

            // Запрашиваем собранный журнал с бэкенда
            var remote = await _restClient.GetAsync<List<FundsAction>>(url);
            return remote ?? Enumerable.Empty<FundsAction>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FUNDS ACTIONS FETCH ERROR]: {ex.Message}");
            return Enumerable.Empty<FundsAction>();
        }
    }
}