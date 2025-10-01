using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class PendingShipmentDetail
    {
        public string PendingShipmentType;
        public ProcessingOptions ProcessingOptions;
        public RecommendedDocumentSpecification RecommendedDocumentSpecification;
        public EmailLabelDetail EmailLabelDetail;
        public List<AttachedDocument> AttachedDocuments;
        public string ExpirationTimeStamp;
    }

}