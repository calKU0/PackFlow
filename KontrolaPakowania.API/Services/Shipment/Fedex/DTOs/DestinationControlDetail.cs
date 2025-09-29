using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class DestinationControlDetail
    {
        public string EndUser;
        public string StatementTypes;
        public List<string> DestinationCountries;
    }

}