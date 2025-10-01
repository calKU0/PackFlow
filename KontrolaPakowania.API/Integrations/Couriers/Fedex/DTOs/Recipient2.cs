using System.Collections.Generic;
namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class Recipient2
    {
        public Address Address;
        public Contact Contact;
        public List<Tin> Tins;
        public string DeliveryInstructions;
    }

}