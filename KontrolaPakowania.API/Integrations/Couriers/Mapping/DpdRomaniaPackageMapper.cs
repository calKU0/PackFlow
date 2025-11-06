using KontrolaPakowania.API.Integrations.Couriers.DPD_Romania.DTOs;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using Microsoft.Extensions.Options;

namespace KontrolaPakowania.API.Integrations.Couriers.Mapping
{
    public class DpdRomaniaPackageMapper : IParcelMapper<DpdRomaniaCreateShipmentRequest>
    {
        private readonly DpdRomaniaSettings _settings;

        public DpdRomaniaPackageMapper(IOptions<CourierSettings> options)
        {
            _settings = options?.Value?.DPDRomania ?? throw new ArgumentNullException(nameof(options));
        }

        public DpdRomaniaCreateShipmentRequest Map(PackageData package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            int countryId = package.RecipientCountry switch
            {
                "RO" => 642,
                "BG" => 100,
                "GR" => 300,
                _ => 642
            };

            // Determine service based on weight/country
            int serviceId = GetServiceId(package.RecipientCountry, package.RecipientCity, package.Weight);

            var shipment = new DpdRomaniaCreateShipmentRequest
            {
                UserName = _settings.Username,
                Password = _settings.Password,
                Service = new()
                {
                    ServiceId = serviceId,
                    AutoAdjustPickupDate = true,
                    AdditionalServices = package.ShipmentServices.COD
                        ? new()
                        {
                            COD = new()
                            {
                                Amount = package.ShipmentServices.CODAmount,
                                CurrencyCode = "RON",
                                OBPDetails = new()
                                {
                                    Option = "OPEN",
                                    ReturnShipmentServiceId = serviceId,
                                    ReturnShipmentPayer = "SENDER"
                                },
                                PayoutToThirdParty = false,
                                ProcessingType = "CASH",
                                IncludeShippingPrice = false
                            }
                        }
                        : null
                },
                Content = new()
                {
                    ParcelsCount = 1,
                    TotalWeight = package.Weight,
                    Contents = "AGRICULTURAL PARTS",
                    Package = package.Weight >= 50 ? "PALLET" : "BOX",
                    Parcels = package.Weight >= 50
                        ? new List<DpdRomaniaCreateShipmentRequest.Parcel>
                        {
                            new()
                            {
                                SeqNo = 1,
                                Size = new()
                                {
                                    Depth = package.Length,
                                    Width = package.Width,
                                    Height = package.Height
                                },
                                Weight = package.Weight
                            }
                        }
                        : null
                },
                Payment = new()
                {
                    CourierServicePayer = "SENDER"
                },
                Recipient = new()
                {
                    Phone1 = new() { Number = package.RecipientPhone },
                    PrivatePerson = true,
                    ClientName = package.RecipientName,
                    ContactName = package.RecipientName,
                    Email = package.RecipientEmail,
                    Address = new()
                    {
                        CountryId = countryId,
                        PostCode = package.RecipientPostalCode,
                        SiteName = package.RecipientCity,
                        StreetType = "str.",
                        StreetName = package.RecipientStreet,
                        StreetNo = " "
                    }
                },
                ShipmentNote = package.Description,
                Ref1 = package.References,
                Ref2 = "R2"
            };

            return shipment;
        }

        private int GetServiceId(string country, string city, decimal weight)
        {
            if (weight < 50)
            {
                if (country == "RO")
                {
                    return city.Equals("oradea", StringComparison.OrdinalIgnoreCase)
                        ? 2114 // LOCO service
                        : 2003;
                }
                else if (country == "BG" || country == "HU" || country == "GR")
                {
                    return 2212;
                }
            }
            else
            {
                if (country == "RO") return 2412;
                if (country == "BG") return 2432;
            }

            return 2003;
        }
    }
}