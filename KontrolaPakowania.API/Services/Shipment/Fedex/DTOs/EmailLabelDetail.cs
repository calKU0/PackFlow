using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class EmailLabelDetail
    {
        public List<Recipient> Recipients;
        public string Message;
    }

}