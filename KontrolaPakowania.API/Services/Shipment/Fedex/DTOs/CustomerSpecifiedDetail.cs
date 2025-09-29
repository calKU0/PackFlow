using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class CustomerSpecifiedDetail
    {
        public List<string> MaskedData;
        public List<RegulatoryLabel> RegulatoryLabels;
        public List<AdditionalLabel> AdditionalLabels;
        public DocTabContent DocTabContent;
    }

}