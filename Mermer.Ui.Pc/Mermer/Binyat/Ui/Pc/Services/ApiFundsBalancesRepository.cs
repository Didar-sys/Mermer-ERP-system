using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Services;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiFundsBalancesRepository : IFundsBalancesRepository
{
    private readonly RestClient _restClient;

    public ApiFundsBalancesRepository(RestClient restClient)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public async Task<FundsBalance> GetBalanceToDateAsync(string depositoryId, DateTime date)
    {
        try
        {
            var depQuery = string.IsNullOrEmpty(depositoryId) ? "" : $"depositoryId={depositoryId}&";
            var url = $"/api/finance/balances/todate?{depQuery}date={date:yyyy-MM-ddTHH:mm:ssZ}";
            return await _restClient.GetAsync<FundsBalance>(url) ?? new FundsBalance { DepositoryId = depositoryId };
        }
        catch
        {
            return new FundsBalance { DepositoryId = depositoryId };
        }
    }

    public async Task<IEnumerable<FundsBalanceByTypeWithBalance>> GetByTypeAsync(string depositoryId, DateTime? dateFrom, DateTime? dateTill)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(depositoryId)) queryParams.Add($"depositoryId={depositoryId}");
            if (dateFrom.HasValue) queryParams.Add($"from={dateFrom.Value:yyyy-MM-ddTHH:mm:ssZ}");
            if (dateTill.HasValue) queryParams.Add($"till={dateTill.Value:yyyy-MM-ddTHH:mm:ssZ}");

            var qs = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var res = await _restClient.GetAsync<List<FundsBalanceByTypeWithBalance>>($"/api/finance/balances/bytype{qs}");
            return res ?? Enumerable.Empty<FundsBalanceByTypeWithBalance>();
        }
        catch
        {
            return Enumerable.Empty<FundsBalanceByTypeWithBalance>();
        }
    }

    public async Task<FundsBalanceAggregated> GetByTypeAggregatedAsync(string[] depositoryIds, DateTime? dateFrom = null, DateTime? dateTill = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (depositoryIds != null && depositoryIds.Any())
            {
                foreach (var d in depositoryIds) queryParams.Add($"depositoryId={d}");
            }
            if (dateFrom.HasValue) queryParams.Add($"from={dateFrom.Value:yyyy-MM-ddTHH:mm:ssZ}");
            if (dateTill.HasValue) queryParams.Add($"till={dateTill.Value:yyyy-MM-ddTHH:mm:ssZ}");

            var qs = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            return await _restClient.GetAsync<FundsBalanceAggregated>($"/api/finance/balances/aggregated{qs}") ?? new FundsBalanceAggregated();
        }
        catch
        {
            return new FundsBalanceAggregated();
        }
    }
}