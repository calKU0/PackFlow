namespace KontrolaPakowania.Server.Settings
{
    public class CrystalReportsOptions
    {
        public string ReportsPath { get; set; } = string.Empty;
        public CrystalDbOptions Database { get; set; } = new();
    }

    public class CrystalDbOptions
    {
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}