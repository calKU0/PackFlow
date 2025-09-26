using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.ServiceModel;

namespace KontrolaPakowania.API.Services.Shipment.GLS
{
    public class GlsService : ICourierService
    {
        private readonly IGlsClientWrapper _client;
        private readonly IParcelMapper<cConsign> _mapper;
        private readonly GlsSettings _settings;

        private string _sessionId;
        private static cParcelWeightsMax _maxWeights;
        private static float _maxCod;
        private Task _loginTask;

        public GlsService(IOptions<CourierSettings> courierSettings, IGlsClientWrapper client, IParcelMapper<cConsign> mapper)
        {
            _settings = courierSettings.Value.GLS;
            _client = client;
            _mapper = mapper;
        }

        private Task EnsureLoggedInAsync()
        {
            if (_loginTask == null)
                _loginTask = LoginAsync();

            return _loginTask;
        }

        private async Task LoginAsync()
        {
            var session = await _client.LoginAsync(_settings.Username, _settings.Password);
            _sessionId = session.session;

            //_maxWeights = await _client.adeServices_GetMaxParcelWeightsAsync(_sessionId);
            //_maxCod = (await _client.adeServices_GetMaxCODAsync(_sessionId)).max_cod;
        }

        public async Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            if (package == null)
                return ShipmentResponse.CreateFailure("Błąd: Nie znaleziono paczki");

            await EnsureLoggedInAsync();

            var parcelData = _mapper.Map(package);

            // Insert parcel
            cID? inserted;
            try
            {
                inserted = await _client.InsertParcelAsync(_sessionId, parcelData);
            }
            catch (FaultException faultEx)
            {
                // Generic SOAP fault
                var msg = $"Błąd danych paczki GLS: {faultEx.Message}";

                if (faultEx.Code != null)
                    msg += $" | Kod: {faultEx.Code.Name}";

                if (faultEx.Reason != null && faultEx.Reason.GetMatchingTranslation().Text != faultEx.Message)
                    msg += $" | Powód: {faultEx.Reason.GetMatchingTranslation().Text}";

                // Sometimes the detail is just in the InnerXml
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
                // Real network/serialization errors
                return ShipmentResponse.CreateFailure($"Błąd systemowy: {ex.Message}");
            }

            if (inserted == null || inserted.id == 0)
                return ShipmentResponse.CreateFailure("Błąd przy próbie wygenerowania paczki GLS.");

            var parcelId = inserted.id;

            // Get label
            adePreparingBox_GetConsignLabelsExtResponse? labels;
            try
            {
                labels = await _client.GetLabelsAsync(_sessionId, parcelId, "roll_160x100_zebra");
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd przy próbie pobrania etykiety paczki GLS: {ex.Message}");
            }

            var label = labels?.@return?.FirstOrDefault();
            if (label == null || string.IsNullOrWhiteSpace(label.number))
                return ShipmentResponse.CreateFailure("Nie zwrócono etykiety do paczki GLS API.");

            return ShipmentResponse.CreateSuccess(
                courier: Courier.GLS,
                packageId: package.Id,
                trackingLink: $"https://gls-group.eu/PL/pl/sledzenie-paczek/?match=={label.number}",
                trackingNumber: label.number,
                labelBase64: label.file,
                labelType: PrintDataType.ZPL,
                packageInfo: package
            );
        }

        public async Task<int> DeletePackageAsync(int parcelId)
        {
            await EnsureLoggedInAsync();

            try
            {
                var deleted = await _client.DeleteParcelAsync(_sessionId, parcelId);
                return deleted.id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task LogoutAsync()
        {
            if (string.IsNullOrEmpty(_sessionId))
                await EnsureLoggedInAsync();

            await _client.LogoutAsync(_sessionId);
        }
    }
}