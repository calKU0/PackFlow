using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.Helpers
{
    public static class CourierHelper
    {
        public static readonly Courier[] AllowedCouriersForLabel =
        {
            Courier.GLS,
            Courier.DPD,
            Courier.DPD_Romania,
            Courier.Fedex
        };
    }
}
