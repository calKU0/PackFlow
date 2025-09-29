using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class ShippingDocumentSpecification
    {
        public GeneralAgencyAgreementDetail GeneralAgencyAgreementDetail;
        public ReturnInstructionsDetail ReturnInstructionsDetail;
        public Op900Detail Op900Detail;
        public UsmcaCertificationOfOriginDetail UsmcaCertificationOfOriginDetail;
        public UsmcaCommercialInvoiceCertificationOfOriginDetail UsmcaCommercialInvoiceCertificationOfOriginDetail;
        public List<string> ShippingDocumentTypes;
        public CertificateOfOrigin CertificateOfOrigin;
        public CommercialInvoiceDetail CommercialInvoiceDetail;
    }

}