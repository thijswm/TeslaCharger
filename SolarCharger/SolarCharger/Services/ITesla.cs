using SolarCharger.Services.Objects;

namespace SolarCharger.Services
{
    public interface ITesla
    {
        int CurrentChargingAmps { get; }
        int CurrentChargePower { get; }
        int CurrentChargeVoltage { get; }
        int CurrentBatteryLevel { get; }
        Task StartAsync();
        Task<bool> IsOnlineAsync();
        Task<VehicleData> GetVehicleDataAsync();
        Task SetChargeAmpsAsync(int amps);
        Task StartChargeAsync();
        Task StopChargeAsync();
        int CalculateChargePower(int phases);
    }
}
