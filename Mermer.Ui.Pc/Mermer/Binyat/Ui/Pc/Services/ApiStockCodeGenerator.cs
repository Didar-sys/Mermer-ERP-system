using System;
using System.Threading.Tasks;
using Mermer.Http;
using Mermer.StockManagement.Services;

namespace Mermer.Ui.Pc.Services
{
    public class StockCodeDto
    {
        public string Code { get; set; }
    }

    public class ApiStockCodeGenerator : IStockCodeGenerationService
    {
        private readonly RestClient _restClient;

        public ApiStockCodeGenerator(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<string> GetNextCode()
        {
            try
            {
                var dto = await _restClient.GetAsync<StockCodeDto>("/api/stocks/next-code");
                return dto?.Code ?? "ST-000001";
            }
            catch
            {
                return "ST-000001";
            }
        }
    }
}