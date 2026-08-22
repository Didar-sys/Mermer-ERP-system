using Mermer.FundsManagement.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiCurrenciesRepository : BaseApiCacheRepository<Currency>
{
    public ApiCurrenciesRepository(RestClient restClient) : base(restClient, "Currency", "currencies") { }
}