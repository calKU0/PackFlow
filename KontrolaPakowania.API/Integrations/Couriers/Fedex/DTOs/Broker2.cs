using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class Broker2
    {
        public Address Address;
        public Contact Contact;
        public AccountNumber AccountNumber;
        public List<Tin> Tins;
        public string DeliveryInstructions;
    }

}