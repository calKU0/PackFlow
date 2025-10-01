using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class CertificateOfOrigin
    {
        public List<CustomerImageUsage> CustomerImageUsages;
        public DocumentFormat DocumentFormat;
    }

}