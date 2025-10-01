using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class UsmcaCommercialInvoiceCertificationOfOriginDetail
    {
        public List<CustomerImageUsage> CustomerImageUsages;
        public DocumentFormat DocumentFormat;
        public string CertifierSpecification;
        public string ImporterSpecification;
        public string ProducerSpecification;
        public Producer Producer;
        public string CertifierJobTitle;
    }

}