using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class ShipmentServices
    {
        public bool POD { get; set; }
        public bool EXW { get; set; }
        public bool ROD { get; set; }
        public bool D10 { get; set; }
        public bool D12 { get; set; }
        public bool PZ { get; set; }
        public bool Dropshipping { get; set; }
        public bool Saturday { get; set; }
        private bool cod;

        public bool COD
        {
            get => cod;
            set
            {
                cod = value;
                if (!cod) CODAmount = 0;
            }
        }

        private decimal codAmount;

        public decimal CODAmount
        {
            get => codAmount;
            set
            {
                codAmount = value;
                cod = codAmount > 0;
            }
        }

        public static ShipmentServices FromString(string input)
        {
            var services = new ShipmentServices();
            var lowerInput = input.ToLower();

            foreach (var kvp in ServiceMapping)
            {
                if (lowerInput.Contains(kvp.Key))
                    kvp.Value(services);
            }

            return services;
        }

        public bool HasAnyService()
        {
            return typeof(ShipmentServices)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(bool))
                .Any(p => (bool)p.GetValue(this));
        }

        private static readonly Dictionary<string, Action<ShipmentServices>> ServiceMapping = new()
        {
            ["10"] = s => s.D10 = true,
            ["12"] = s => s.D12 = true,
            ["sobota"] = s => s.Saturday = true,
            ["zwrotna"] = s => s.PZ = true,
            ["dropshipping"] = s => s.Dropshipping = true
        };
    }
}