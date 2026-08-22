using Mermer.Enterprise.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiOfficesRepository : BaseApiCacheRepository<Office>
{
    public ApiOfficesRepository(RestClient restClient) : base(restClient, "Office", "offices") { }
}