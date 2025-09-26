using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.Shipment
{
    public interface ICourierService
    {
        Task<ShipmentResponse> SendPackageAsync(PackageData package);

        Task<int> DeletePackageAsync(int packageId);
    }
}