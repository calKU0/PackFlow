using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class EMailDetail
    {
        public List<EMailRecipient> EMailRecipients;
        public string Locale;
        public string Grouping;
    }

}