using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class SoldTo
    {
        public Address Address;
        public Contact Contact;
        public List<Tin> Tins;
        public AccountNumber AccountNumber;
    }

}