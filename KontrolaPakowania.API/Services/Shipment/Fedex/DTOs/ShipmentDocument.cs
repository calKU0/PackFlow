using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class ShipmentDocument
    {
        public string ContentKey;
        public int CopiesToPrint;
        public string ContentType;
        public string TrackingNumber;
        public string DocType;
        public List<object> Alerts;
        public string EncodedLabel;
        public string Url;
    }

}