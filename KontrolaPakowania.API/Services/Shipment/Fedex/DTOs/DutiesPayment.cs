namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs
{
    public class DutiesPayment
    {
        public Payor Payor { get; set; }
        public BillingDetails BillingDetails { get; set; }
        public string PaymentType { get; set; }
    }
}