using KontrolaPakowania.API.Services.Shipment.Fedex.DTOs;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KontrolaPakowania.API.Services.Shipment.Fedex.Strategies
{
    public class FedexRestStrategy : IFedexApiStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly IFedexTokenService _tokenService;
        private readonly FedexRestSettings _restSettings;
        private readonly IParcelMapper<FedexShipmentRequest> _mapper;

        public FedexRestStrategy(HttpClient httpClient, IFedexTokenService tokenService, IParcelMapper<FedexShipmentRequest> mapper, IOptions<CourierSettings> courierSettings)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _mapper = mapper;
            _restSettings = courierSettings.Value.Fedex.Rest;
        }

        public async Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            if (package == null)
                return ShipmentResponse.CreateFailure("Błąd: Brak danych paczki.");

            FedexShipmentRequest fedexRequest;
            try
            {
                fedexRequest = _mapper.Map(package);
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd mapowania paczki do formatu FedEx REST: {ex.Message}");
            }

            try
            {
                var token = await _tokenService.GetTokenAsync();
                var jsonSettings = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(fedexRequest, jsonSettings);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var request = new HttpRequestMessage(HttpMethod.Post, "ship/v1/shipments")
                {
                    Content = content
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("X-locale", "pl_PL");

                var response = await _httpClient.SendAsync(request);

                var responseContent = await ReadResponseAsync(response);

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        Console.WriteLine("Raw FedEx error:");
                        Console.WriteLine(responseContent);
                        var errorResponse = JsonSerializer.Deserialize<FedexErrorResponse>(
                            responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (errorResponse?.Errors != null && errorResponse.Errors.Any())
                        {
                            var errorMessages = errorResponse.Errors
                                .Select(e =>
                                {
                                    var parameters = e.ParameterList != null
                                        ? string.Join(", ", e.ParameterList.Select(p => $"{p.Key}={p.Value}"))
                                        : string.Empty;

                                    return $"{e.Code}: {e.Message}" +
                                           (string.IsNullOrEmpty(parameters) ? "" : $" ({parameters})");
                                });

                            return ShipmentResponse.CreateFailure(
                                $"FedEx REST API request failed ({response.StatusCode}): " +
                                string.Join(" | ", errorMessages)
                            );
                        }
                    }
                    catch
                    {
                        // fallback to raw response
                    }

                    return ShipmentResponse.CreateFailure($"FedEx REST API request failed: {response.StatusCode} | {responseContent}");
                }

                var root = JsonSerializer.Deserialize<FedexShipmentResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (root?.Output?.TransactionShipments == null || !root.Output.TransactionShipments.Any())
                    return ShipmentResponse.CreateFailure("FedEx REST API nie zwrócił żadnych przesyłek.");

                var firstShipment = root.Output.TransactionShipments.First();
                var piece = firstShipment.PieceResponses?.FirstOrDefault();
                var document = piece?.PackageDocuments?.FirstOrDefault();

                if (document == null)
                    return ShipmentResponse.CreateFailure("FedEx REST API nie zwrócił etykiety.");

                return ShipmentResponse.CreateSuccess(
                    courier: Courier.Fedex,
                    packageId: package.Id,
                    trackingLink: $"https://www.fedex.com/fedextrack/?trknbr={firstShipment.MasterTrackingNumber}",
                    trackingNumber: firstShipment.MasterTrackingNumber,
                    labelBase64: document.EncodedLabel,
                    labelType: PrintDataType.ZPL,
                    packageInfo: package
                );
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd FedEx REST API: {ex.Message}");
            }
        }

        private static async Task<string> ReadResponseAsync(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();

            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                using var gzip = new GZipStream(stream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzip, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
            {
                using var deflate = new DeflateStream(stream, CompressionMode.Decompress);
                using var reader = new StreamReader(deflate, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            else
            {
                using var reader = new StreamReader(stream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
        }
    }
}