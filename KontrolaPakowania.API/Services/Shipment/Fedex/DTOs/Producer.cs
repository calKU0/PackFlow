using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class Producer
    {
        public Address Address;
        public Contact Contact;
        public AccountNumber AccountNumber;
        public List<Tin> Tins;
    }

}