namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class ContentRecord
    {
        public string ItemNumber { get; set; }
        public int ReceivedQuantity { get; set; }
        public string Description { get; set; }
        public string PartNumber { get; set; }
    }
}