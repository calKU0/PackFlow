using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class DangerousGoodsDetail
    {
        public bool CargoAircraftOnly;
        public string Regulation;
        public string Accessibility;
        public List<string> Options;
    }

}