using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class EtdDetail
    {
        public List<string> Attributes;
        public List<AttachedDocument> AttachedDocuments;
        public List<string> RequestedDocumentTypes;
    }

}