using KontrolaPakowania.API.Integrations.Couriers.DPD_Romania.DTOs;
using KontrolaPakowania.API.Integrations.Couriers.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace KontrolaPakowania.API.Integrations.Couriers.DPD_Romania
{
    public class DpdRomaniaService : ICourierService
    {
        private readonly HttpClient _httpClient;
        private readonly IParcelMapper<DpdRomaniaCreateShipmentRequest> _mapper;
        private readonly DpdRomaniaSettings _settings;

        public DpdRomaniaService(HttpClient httpClient, IParcelMapper<DpdRomaniaCreateShipmentRequest> mapper, IOptions<DpdRomaniaSettings> options)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            try
            {
                if (package == null)
                    return ShipmentResponse.CreateFailure("Brak danych paczki.");

                var requestBody = _mapper.Map(package);
                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/v1/shipment", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                var shipmentResponse = JsonSerializer.Deserialize<DpdRomaniaCreateShipmentResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode)
                {
                    if (shipmentResponse?.Error != null)
                    {
                        return ShipmentResponse.CreateFailure(BuildErrorMessage(shipmentResponse.Error));
                    }

                    return ShipmentResponse.CreateFailure($"DPD Romania API HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {responseJson}");
                }

                // handle API-level error
                if (shipmentResponse == null)
                    return ShipmentResponse.CreateFailure("Pusta odpowiedź z DPD Romania API.");

                if (shipmentResponse.Error != null)
                {
                    return ShipmentResponse.CreateFailure(BuildErrorMessage(shipmentResponse.Error));
                }

                if (string.IsNullOrWhiteSpace(shipmentResponse.Id))
                    return ShipmentResponse.CreateFailure("DPD Romania API nie zwróciło ID przesyłki.");

                // try to fetch label
                var label = await GetShipmentLabelAsync(shipmentResponse.Id);
                if (label == null)
                    return ShipmentResponse.CreateFailure("Nie udało się pobrać etykiety z DPD Romania API.");

                return ShipmentResponse.CreateSuccess(
                    courier: Courier.DPD_Romania,
                    packageId: package.Id,
                    trackingNumber: shipmentResponse.Id,
                    trackingLink: $"https://tracking.dpd.ro/{shipmentResponse.Id}",
                    labelBase64: label,
                    labelType: PrintDataType.PDF,
                    packageInfo: package
                );
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd tworzenia paczki DPD Romania: {ex.Message}");
            }
        }

        private async Task<string?> GetShipmentLabelAsync(string shipmentId)
        {
            var request = new
            {
                userName = _settings.Username,
                password = _settings.Password,
                paperSize = "A6",
                format = "zpl",
                parcels = new[]
                {
                    new { parcel = new { id = shipmentId } }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v1/print", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return Convert.ToBase64String(bytes);
        }

        public async Task<int> DeletePackageAsync(int packageId)
        {
            var request = new
            {
                userName = _settings.Username,
                password = _settings.Password,
                shipmentId = packageId.ToString(),
                comment = "Cancel from API"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v1/shipment/cancel", content);

            if (!response.IsSuccessStatusCode)
                return -1;

            var result = await response.Content.ReadAsStringAsync();
            return result.Contains("{}") ? 1 : -1;
        }

        private string BuildErrorMessage(DpdRomaniaError error)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(error.Message))
                parts.Add(error.Message);

            if (error.Code != 0)
                parts.Add($"Kod błędu: {error.Code}");

            if (!string.IsNullOrWhiteSpace(error.Id))
                parts.Add($"Id błędu: {error.Id}");

            return "DPD Romania API zwróciło błąd: " + string.Join("; ", parts);
        }
    }
}