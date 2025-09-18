using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.Shipment
{
    public interface IShipmentService
    {
        Task<int> CreateErpShipmentDocument(ShipmentResponse shipment);

        Task<bool> AddErpAttributes(int documentId, PackageInfo packageInfo);
    }
}