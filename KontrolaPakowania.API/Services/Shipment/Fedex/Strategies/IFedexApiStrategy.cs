using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.Shipment.Fedex.Strategies
{
    public interface IFedexApiStrategy
    {
        Task<ShipmentResponse> SendPackageAsync(PackageData package);
    }
}