using KontrolaPakowania.API.Services.Shipment;
using KontrolaPakowania.Shared.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using System;

namespace KontrolaPakowania.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly CourierFactory _courierFactory;
        private readonly IShipmentService _shipmentService;

        public ShipmentsController(CourierFactory courierFactory, IShipmentService shipmentService)
        {
            _courierFactory = courierFactory;
            _shipmentService = shipmentService;
        }

        [HttpPost("create-shipment")]
        public async Task<IActionResult> CreateShipment([FromBody] ShipmentRequest request)
        {
            try
            {
                var courier = _courierFactory.GetCourier(request.Courier);
                var result = await courier.SendPackageAsync(request);
                if (!result.Success)
                    return Ok(result);

                var createDocResult = await _shipmentService.CreateErpShipmentDocument(result);
                if (createDocResult <= 0)
                    return StatusCode(500, "Nie udało się założyć wysyłki dokumentu w ERP.");

                result.ErpShipmentId = createDocResult;
                if (result.ErpShipmentId > 0 && result.Success)
                {
                    await _shipmentService.AddErpAttributes(result.ErpShipmentId, result.PackageInfo);
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}