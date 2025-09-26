using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Services.Shipment.Mapping
{
    public class GlsParcelMapper : IParcelMapper<cConsign>
    {
        public cConsign Map(PackageData package)
        {
            return new cConsign
            {
                rname1 = package.RecipientName,
                rcountry = package.RecipientCountry,
                rzipcode = package.RecipientPostalCode,
                rcity = package.RecipientCity,
                rstreet = package.RecipientStreet,
                rphone = package.RecipientPhone,
                rcontact = package.RecipientEmail,
                notes = package.Description,
                references = package.References,
                quantity = package.PackageQuantity,
                quantitySpecified = true,
                weight = (float)package.Weight,
                weightSpecified = true,
                srv_bool = new cServicesBool
                {
                    pod = package.ShipmentServices.POD,
                    podSpecified = package.ShipmentServices.POD,
                    exw = package.ShipmentServices.EXW,
                    exwSpecified = package.ShipmentServices.EXW,
                    cod = package.ShipmentServices.COD,
                    codSpecified = package.ShipmentServices.COD,
                    cod_amount = (float)package.ShipmentServices.CODAmount,
                    cod_amountSpecified = package.ShipmentServices.COD
                }
            };
        }
    }
}