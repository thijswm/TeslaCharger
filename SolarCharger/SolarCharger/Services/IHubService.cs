using SolarCharger.Services.Objects;

namespace SolarCharger.Services
{
    public interface IHubService
    {
        Task SendStateChangedAsync(eState state);
        Task SendVehicleDataAsync(VehicleData data);
    }
}
