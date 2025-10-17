using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using System.Text.Json;

namespace KontrolaPakowania.API.Integrations.Wms
{
    public class WmsApiClient : IWmsApiClient
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public WmsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<JlDto>> GetJlListAsync(CancellationToken cancellationToken = default)
        {
            var request = new { warehouseId = "6" };
            var response = await _httpClient.PostAsJsonAsync("wms-int-api/companies/62/integrations/own/service?integrationName=getLuToPack", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<IEnumerable<JlDto>>(cancellationToken);
            return data ?? Enumerable.Empty<JlDto>();
        }

        public async Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jlCode, CancellationToken cancellationToken = default)
        {
            var request = new { jlCode };
            var response = await _httpClient.PostAsJsonAsync("wms-int-api/companies/62/integrations/own/service?integrationName=getLuItems", request, cancellationToken);

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<IEnumerable<JlItemDto>>(cancellationToken);
            return data ?? Enumerable.Empty<JlItemDto>();
        }

        public async Task<PackWMSResponse> PackStock(PackStockRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("wms-int-api/companies/62/integrations/own/service?integrationName=packStock", request, _jsonOptions, cancellationToken);

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<PackWMSResponse>(_jsonOptions, cancellationToken);

            return data ?? new PackWMSResponse();
        }

        public async Task<PackWMSResponse> CloseJl(CloseLuRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("wms-int-api/companies/62/integrations/own/service?integrationName=closeLu", request, _jsonOptions, cancellationToken);

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<PackWMSResponse>(_jsonOptions, cancellationToken);

            return data ?? new PackWMSResponse();
        }
    }
}