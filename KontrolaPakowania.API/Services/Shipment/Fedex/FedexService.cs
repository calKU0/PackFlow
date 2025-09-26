using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.Shipment.Fedex
{
    public class FedexService : ICourierService
    {
        public Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeletePackageAsync(int packageId)
        {
            // No need to delete package in Fedex
            return Task.FromResult(1);
        }
    }
}