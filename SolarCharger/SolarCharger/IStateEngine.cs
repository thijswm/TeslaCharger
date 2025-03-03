using SolarCharger.Services.Objects;

namespace SolarCharger
{
    public interface IStateEngine
    {
        eState State { get; }
        VehicleData? LatestVehicleData { get; }
        Task FireStartAsync();
        Task FireStopAsync();
        List<PowerHistory> GetPowerHistory();
    }
}
