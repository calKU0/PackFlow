using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class ShipmentSpecialServices
    {
        public List<string> SpecialServiceTypes;
        public EtdDetail EtdDetail;
        public ReturnShipmentDetail ReturnShipmentDetail;
        public DeliveryOnInvoiceAcceptanceDetail DeliveryOnInvoiceAcceptanceDetail;
        public InternationalTrafficInArmsRegulationsDetail InternationalTrafficInArmsRegulationsDetail;
        public PendingShipmentDetail PendingShipmentDetail;
        public HoldAtLocationDetail HoldAtLocationDetail;
        public ShipmentCODDetail ShipmentCODDetail;
        public ShipmentDryIceDetail ShipmentDryIceDetail;
        public InternationalControlledExportDetail InternationalControlledExportDetail;
        public HomeDeliveryPremiumDetail HomeDeliveryPremiumDetail;
    }

}