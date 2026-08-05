using System.Collections.Generic;
using System.Threading.Tasks;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services;

public class StocksApiClient
{
    private readonly RestClient _restClient;

    public StocksApiClient(RestClient restClient)
    {
        _restClient = restClient;
    }

    /// <summary>
    /// Получить все остатки/склады через HTTP API
    /// </summary>
    public async Task<List<TStockDto>> GetAllStocksAsync<TStockDto>()
    {
        // Вызов маршрута /api/stocks, определенного в MapStocksEndpoints()
        return await _restClient.GetAsync<List<TStockDto>>("api/stocks");
    }

    /// <summary>
    /// Запустить синхронизацию
    /// </summary>
    public async Task PushSyncAsync<TSyncData>(TSyncData syncData)
    {
        // Вызов маршрута /api/sync/push, определенного в MapSyncEndpoints()
        await _restClient.PostAsync("api/sync/push", syncData);
    }
}