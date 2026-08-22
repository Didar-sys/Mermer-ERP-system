using Mermer.Enterprise.Models;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class ApiWarehousesRepository : BaseApiCacheRepository<Warehouse>
{
    public ApiWarehousesRepository(RestClient restClient) : base(restClient, "Warehouse", "warehouses") { }
}