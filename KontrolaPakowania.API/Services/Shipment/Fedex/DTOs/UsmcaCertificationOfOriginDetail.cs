using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class UsmcaCertificationOfOriginDetail
    {
        public List<CustomerImageUsage> CustomerImageUsages;
        public DocumentFormat DocumentFormat;
        public string CertifierSpecification;
        public string ImporterSpecification;
        public string ProducerSpecification;
        public Producer Producer;
        public BlanketPeriod BlanketPeriod;
        public string CertifierJobTitle;
    }

}