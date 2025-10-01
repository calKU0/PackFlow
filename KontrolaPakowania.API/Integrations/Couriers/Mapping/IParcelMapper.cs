using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Integrations.Couriers.Mapping
{
    public interface IParcelMapper<TParcel>
    {
        TParcel Map(PackageData package);
    }
}