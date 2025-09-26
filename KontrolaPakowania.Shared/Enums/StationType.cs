using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.Enums
{
    public enum StationType
    {
        [Description("Pakowanie")]
        Packing,
        [Description("Wysyłka Paczek")]
        Shipping,
        [Description("Monitorowanie")]
        Monitoring
    }
}
