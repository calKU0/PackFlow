namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs{ 

    public class ShipmentCODDetail
    {
        public AddTransportationChargesDetail AddTransportationChargesDetail;
        public CodRecipient CodRecipient;
        public string RemitToName;
        public string CodCollectionType;
        public FinancialInstitutionContactAndAddress FinancialInstitutionContactAndAddress;
        public CodCollectionAmount CodCollectionAmount;
        public string ReturnReferenceIndicatorType;
        public ShipmentCodAmount ShipmentCodAmount;
    }

}