using SolarCharger.Services.Objects;

namespace SolarCharger
{
    public interface IStateEngine
    {
        eState State { get; }
        VehicleData? LatestVehicleData { get; }
        void FireIdleLoop();
        Task FireStartAsync();
        Task FireStopAsync();
        List<PowerHistory> GetPowerHistory();
    }
}
