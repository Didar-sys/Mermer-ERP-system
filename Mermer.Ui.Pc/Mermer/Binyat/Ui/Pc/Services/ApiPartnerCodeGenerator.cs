using System;
using System.Threading.Tasks;
using Mermer.CRM.Services;
using Mermer.Http;

namespace Mermer.Ui.Pc.Services
{
    public class PartnerCodeDto
    {
        public string Code { get; set; }
    }

    public class ApiPartnerCodeGenerator : IPartnerCodeGenerationService
    {
        private readonly RestClient _restClient;

        public ApiPartnerCodeGenerator(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<string> GetNextCode()
        {
            try
            {
                var dto = await _restClient.GetAsync<PartnerCodeDto>("/api/catalog/partners/next-code");
                return dto?.Code ?? "P-00001";
            }
            catch
            {
                return "P-00001";
            }
        }
    }
}