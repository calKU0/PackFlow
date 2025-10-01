using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class EmailLabelDetail
    {
        public List<Recipient> Recipients;
        public string Message;
    }

}