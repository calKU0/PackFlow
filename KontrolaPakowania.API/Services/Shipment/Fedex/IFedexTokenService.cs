namespace KontrolaPakowania.API.Services.Shipment.Fedex
{
    public interface IFedexTokenService
    {
        Task<string> GetTokenAsync();
    }
}