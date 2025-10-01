using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class EtdDetail
    {
        public List<string> Attributes;
        public List<AttachedDocument> AttachedDocuments;
        public List<string> RequestedDocumentTypes;
    }

}