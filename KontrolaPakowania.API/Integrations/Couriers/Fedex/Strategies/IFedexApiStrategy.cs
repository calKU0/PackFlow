using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.Strategies
{
    public interface IFedexApiStrategy
    {
        Task<ShipmentResponse> SendPackageAsync(PackageData package);
        Task<string> GenerateProtocol(IEnumerable<RoutePackages> shipments);
    }
}