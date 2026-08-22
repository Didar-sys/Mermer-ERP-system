using Mermer.Enterprise.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiDepositoriesRepository : BaseApiCacheRepository<Depository>
{
    public ApiDepositoriesRepository(RestClient restClient) : base(restClient, "Depository", "depositories") { }
}