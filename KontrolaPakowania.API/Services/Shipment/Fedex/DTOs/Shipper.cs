using System.Collections.Generic;

namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs
{
    public class Shipper
    {
        public Address Address { get; set; }
        public Contact Contact { get; set; }
    }
}