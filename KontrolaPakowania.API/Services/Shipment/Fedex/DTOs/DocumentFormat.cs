using System.Collections.Generic; 
namespace KontrolaPakowania.API.Services.Shipment.Fedex.DTOs{ 

    public class DocumentFormat
    {
        public bool ProvideInstructions;
        public OptionsRequested OptionsRequested;
        public string StockType;
        public List<Disposition> Dispositions;
        public string Locale;
        public string DocType;
    }

}