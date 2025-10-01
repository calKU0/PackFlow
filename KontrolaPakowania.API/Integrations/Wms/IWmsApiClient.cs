using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Integrations.Wms
{
    public interface IWmsApiClient
    {
        Task<IEnumerable<JlDto>> GetJlListAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jlCode, CancellationToken cancellationToken = default);
    }
}