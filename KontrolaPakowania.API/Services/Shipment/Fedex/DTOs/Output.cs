using System.Collections.Generic;

namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs
{
    public class Output
    {
        public List<TransactionShipment> TransactionShipments { get; set; }
        public List<Alert> Alerts { get; set; }
        public string JobId { get; set; }
    }
}