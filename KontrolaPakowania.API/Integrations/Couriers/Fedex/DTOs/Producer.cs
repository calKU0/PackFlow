using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class Producer
    {
        public Address Address;
        public Contact Contact;
        public AccountNumber AccountNumber;
        public List<Tin> Tins;
    }

}