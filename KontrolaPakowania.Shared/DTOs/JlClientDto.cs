using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class JlClientDto
    {
        [JsonPropertyName("courier")]
        public string CourierName { get; set; } = string.Empty;

        private Courier courier;

        [JsonIgnore]
        public Courier Courier
        {
            get => courier;
            set
            {
                if (courier != value)
                {
                    courier = value;
                    InitCourierLogo();
                }
            }
        }

        [JsonIgnore]
        public string LogoCourier { get; set; } = string.Empty;

        public string ClientErpId { get; set; } = string.Empty;
        public int? ClientErpAddressId { get; set; }
        public string DestinationCountry { get; set; } = string.Empty;

        [JsonIgnore]
        public ShipmentServices ShipmentServices { get; set; } = new();

        [JsonIgnore]
        public bool PackageClosed { get; set; }

        [JsonIgnore]
        public string PackingRequirements { get; set; } = string.Empty;

        private void InitCourierLogo()
        {
            var suffixes = new List<string>();

            foreach (var prop in typeof(ShipmentServices).GetProperties())
            {
                if (prop.PropertyType == typeof(bool) && (bool)prop.GetValue(ShipmentServices))
                {
                    suffixes.Add(prop.Name);
                }
            }

            LogoCourier = suffixes.Any()
                ? $"{Courier.GetDescription()}-{string.Join(", ", suffixes)}"
                : Courier.GetDescription();
        }
    }
}