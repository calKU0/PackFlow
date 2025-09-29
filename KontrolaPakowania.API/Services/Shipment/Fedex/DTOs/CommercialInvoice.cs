using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class CommercialInvoice
    {
        public string OriginatorName;
        public List<string> Comments;
        public List<CustomerReference> CustomerReferences;
        public TaxesOrMiscellaneousCharge TaxesOrMiscellaneousCharge;
        public string TaxesOrMiscellaneousChargeType;
        public FreightCharge FreightCharge;
        public PackingCosts PackingCosts;
        public HandlingCosts HandlingCosts;
        public string DeclarationStatement;
        public string TermsOfSale;
        public string SpecialInstructions;
        public string ShipmentPurpose;
        public EmailNotificationDetail EmailNotificationDetail;
    }

}