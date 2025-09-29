using FedexServiceReference;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Options;
using System.ServiceModel;

namespace KontrolaPakowania.API.Services.Shipment.Fedex.Strategies
{
    public class FedexSoapStrategy : IFedexApiStrategy
    {
        private readonly IFedexClientWrapper _client;
        private readonly IParcelMapper<listV2> _mapper;
        private readonly FedexSoapSettings _soapSettings;

        public FedexSoapStrategy(IFedexClientWrapper client, IParcelMapper<listV2> mapper, IOptions<CourierSettings> courierSettings)
        {
            _client = client;
            _mapper = mapper;
            _soapSettings = courierSettings.Value.Fedex.Soap;
        }

        public async Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            if (package == null)
                return ShipmentResponse.CreateFailure("Błąd: Brak danych paczki.");

            listV2 fedexRequest;
            try
            {
                fedexRequest = _mapper.Map(package);
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd mapowania paczki do formatu FedEx: {ex.Message}");
            }

            try
            {
                // Insert shipment
                var result = await _client.zapiszListV2Async(_soapSettings.AccessCode, fedexRequest);
                if (result == null || string.IsNullOrWhiteSpace(result.waybill))
                {
                    return ShipmentResponse.CreateFailure("FedEx API nie zwrócił numeru przesyłki.");
                }

                // Download label
                var labelBytes = await _client.wydrukujEtykieteAsync(_soapSettings.AccessCode, result.waybill, "ZPL200");
                if (labelBytes == null || labelBytes.Length == 0)
                {
                    return ShipmentResponse.CreateFailure("FedEx API nie zwrócił etykiety.");
                }

                return ShipmentResponse.CreateSuccess(
                    courier: Courier.Fedex,
                    packageId: package.Id,
                    trackingLink: $"https://www.fedex.com/fedextrack/?trknbr={result.waybill}",
                    trackingNumber: result.waybill,
                    labelBase64: Convert.ToBase64String(labelBytes),
                    labelType: PrintDataType.ZPL,
                    packageInfo: package
                );
            }
            catch (FaultException faultEx)
            {
                var msg = $"Błąd danych paczki FedEx: {faultEx.Message}";

                if (faultEx.Code != null)
                    msg += $" | Kod: {faultEx.Code.Name}";

                if (faultEx.Reason != null && faultEx.Reason.GetMatchingTranslation().Text != faultEx.Message)
                    msg += $" | Powód: {faultEx.Reason.GetMatchingTranslation().Text}";

                if (faultEx.CreateMessageFault().HasDetail)
                {
                    using var reader = faultEx.CreateMessageFault().GetReaderAtDetailContents();
                    string detailText = reader.ReadContentAsString();
                    msg += $" | Szczegóły: {detailText}";
                }

                return ShipmentResponse.CreateFailure(msg);
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd FedEx SOAP API: {ex.Message}");
            }
        }
    }
}