using System.Collections.Generic;

namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class CustomsClearanceDetail
    {
        public List<string> RegulatoryControls { get; set; }
        public List<Broker> Brokers { get; set; }
        public CommercialInvoice CommercialInvoice { get; set; }
        public string FreightOnValue { get; set; }
        public DutiesPayment DutiesPayment { get; set; }
        public List<Commodity> Commodities { get; set; }
        public bool IsDocumentOnly { get; set; }
        public RecipientCustomsId RecipientCustomsId { get; set; }
        public CustomsOption CustomsOption { get; set; }
        public ImporterOfRecord ImporterOfRecord { get; set; }
        public string GeneratedDocumentLocale { get; set; }
        public ExportDetail ExportDetail { get; set; }
        public TotalCustomsValue TotalCustomsValue { get; set; }
        public bool PartiesToTransactionAreRelated { get; set; }
        public DeclarationStatementDetail DeclarationStatementDetail { get; set; }
        public InsuranceCharge InsuranceCharge { get; set; }
    }
}