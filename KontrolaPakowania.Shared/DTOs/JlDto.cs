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
    public class JlDto
    {
        public int JlId { get; set; }
        public string JlCode { get; set; } = string.Empty;
        public string JlEanCode { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusSymbol { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string DestZone { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public string ReadyToPack { get; set; } = string.Empty;
        public List<JlClientDto> Clients { get; set; } = new();
    }
}