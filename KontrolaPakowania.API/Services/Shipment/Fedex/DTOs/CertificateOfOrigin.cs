using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class CertificateOfOrigin
    {
        public List<CustomerImageUsage> CustomerImageUsages;
        public DocumentFormat DocumentFormat;
    }

}