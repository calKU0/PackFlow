using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class EmailNotificationDetail
    {
        public string AggregationType;
        public List<EmailNotificationRecipient> EmailNotificationRecipients;
        public string PersonalMessage;
        public string EmailAddress;
        public string Type;
        public string RecipientType;
    }

}