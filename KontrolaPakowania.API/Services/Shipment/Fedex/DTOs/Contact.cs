namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs
{
    public class Contact
    {
        public string PersonName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneExtension { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public int FaxNumber { get; set; }
    }
}