namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs
{
    public class VariableHandlingChargeDetail
    {
        public string RateType { get; set; }
        public double PercentValue { get; set; }
        public string RateLevelType { get; set; }
        public FixedValue FixedValue { get; set; }
        public string RateElementBasis { get; set; }
    }
}