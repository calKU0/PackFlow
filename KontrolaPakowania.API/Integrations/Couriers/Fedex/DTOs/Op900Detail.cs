using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class Op900Detail
    {
        public List<CustomerImageUsage> CustomerImageUsages;
        public string SignatureName;
        public DocumentFormat DocumentFormat;
    }

}