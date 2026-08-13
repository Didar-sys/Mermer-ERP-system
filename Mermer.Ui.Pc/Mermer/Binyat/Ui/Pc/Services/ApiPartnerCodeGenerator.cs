using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mermer.CRM.Services;

namespace Mermer.Ui.Pc.Services
{
    public class ApiPartnerCodeGenerator : IPartnerCodeGenerationService
    {
        private readonly HttpClient _httpClient;

        public ApiPartnerCodeGenerator(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetNextCode()
        {
            try
            {
                // Жесткий таймаут 200 миллисекунд для моментального фолбэка в оффлайне
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
                var response = await _httpClient.GetAsync("/api/partners/next-code", cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    if (doc.RootElement.TryGetProperty("code", out var codeProp))
                    {
                        return codeProp.GetString();
                    }
                }
            }
            catch
            {
                // Сервер недоступен — мгновенно уходим в фолбэк
            }

            return $"P-{DateTime.Now:yyMMddHHmmss}";
        }
    }
}