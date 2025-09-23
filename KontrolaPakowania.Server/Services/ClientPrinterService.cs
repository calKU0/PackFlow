using KontrolaPakowania.Server.Settings;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace KontrolaPakowania.Server.Services
{
    public class ClientPrinterService
    {
        private readonly IJSRuntime _js;
        private readonly CrystalReportsOptions _crystalOptions;

        public ClientPrinterService(IJSRuntime js, IOptions<CrystalReportsOptions> crystalOptions)
        {
            _js = js;
            _crystalOptions = crystalOptions.Value;
        }

        public async Task<bool> PrintAsync(string printer, string dataType, string content)
        {
            try
            {
                return await _js.InvokeAsync<bool>("sendZplToAgent", printer, dataType, content);
            }
            catch (JSException ex)
            {
                Console.WriteLine($"JS Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PrintCrystalAsync(string printer, string internalBarcode)
        {
            var parameters = new Dictionary<string, string>
            {
                { "DbUser", _crystalOptions.Database.User },
                { "DbPassword", _crystalOptions.Database.Password },
                { "DbServer", _crystalOptions.Database.Server },
                { "DbName", _crystalOptions.Database.Name },
                { "SelectionFormula", $"{{KontrolaPakowania.KP_KodKreskowyWewnetrzny}} = '{internalBarcode}'" }
            };

            return await _js.InvokeAsync<bool>(
                "sendZplToAgent",
                printer,
                PrintDataType.CRYSTAL.ToString(),
                _crystalOptions.ReportsPath,
                parameters
            );
        }
    }
}