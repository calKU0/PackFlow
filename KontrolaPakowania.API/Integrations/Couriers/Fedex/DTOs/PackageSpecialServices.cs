using System.Collections.Generic;

namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class PackageSpecialServices
    {
        public List<string> SpecialServiceTypes { get; set; }
        public string SignatureOptionType { get; set; }
        public PriorityAlertDetail PriorityAlertDetail { get; set; }
        public SignatureOptionDetail SignatureOptionDetail { get; set; }
        public AlcoholDetail AlcoholDetail { get; set; }
        public DangerousGoodsDetail DangerousGoodsDetail { get; set; }
        public PackageCODDetail PackageCODDetail { get; set; }
        public int PieceCountVerificationBoxCount { get; set; }
        public List<BatteryDetail> BatteryDetails { get; set; }
        public DryIceWeight DryIceWeight { get; set; }
        public List<StandaloneBatteryDetail> StandaloneBatteryDetails { get; set; }
    }
}