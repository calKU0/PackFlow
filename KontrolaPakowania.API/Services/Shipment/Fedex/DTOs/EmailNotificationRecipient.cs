using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class EmailNotificationRecipient
    {
        public string Name;
        public string EmailNotificationRecipientType;
        public string EmailAddress;
        public string NotificationFormatType;
        public string NotificationType;
        public string Locale;
        public List<string> NotificationEventType;
    }

}