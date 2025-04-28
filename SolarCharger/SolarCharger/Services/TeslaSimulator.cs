using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolarCharger.EF;
using SolarCharger.Services.Objects;

namespace SolarCharger.Services
{
    public class TeslaSimulator : ITesla
    {
        private readonly ILogger<TeslaSimulator> _log;
        private readonly ChargeContext _chargeContext;
        private Settings? _settings;

        public TeslaSimulator(ILogger<TeslaSimulator> log, ChargeContext chargeContext)
        {
            _log = log;
            _chargeContext = chargeContext;
            CurrentChargingAmps = 0;
        }

        public int CurrentChargingAmps { get; set; }
        public int CurrentChargeVoltage => 230;
        public int CurrentBatteryLevel => 41;
        public int CurrentChargePower => (int)(CurrentChargingAmps * 1.73 * CurrentChargeVoltage);

        public Task<string?> GetRefreshTokenAsync()
        {
            return null;
        }

        public async Task StartAsync()
        {
            try
            {
                _settings = await _chargeContext.Settings.FirstOrDefaultAsync();
            }
            catch (Exception exp)
            {
                throw new Exception($"Failed to get settings, Error: '{exp.Message}'");
            }
        }

        public async Task<bool> IsOnlineAsync()
        {
            await Task.Delay(100);
            return true;
        }

        public async Task<Tuple<string, VehicleData>> GetVehicleDataAsync()
        {
            await Task.Delay(100);

            var vehicleData = new VehicleData
            {
                ChargeState = new VehicleDataChargeState
                {
                    BatteryLevel = 41,
                    ChargeAmps = 16,
                    ChargeCurrentRequest = 16,
                    ChargeCurrentRequestMax = 16,
                    ChargePortLatch = "Engaged",
                    ChargerPower = CurrentChargePower / 1000,
                    ChargeLimitSoc = 80
                }
            };

            return new Tuple<string, VehicleData>("", vehicleData);
        }

        public async Task SetChargeAmpsAsync(int amps)
        {
            CurrentChargingAmps = amps;
            await Task.Delay(100);
            _log.LogInformation("Set charging amps to: {amps}", amps);
        }

        public async Task StartChargeAsync()
        {
            _log.LogInformation("Start charging");
            await Task.Delay(100);
        }

        public async Task StopChargeAsync()
        {
            _log.LogInformation("Stop charging");
            await Task.Delay(100);
        }

        public int CalculateChargePower(int phases)
        {
            return CurrentChargingAmps * phases * CurrentChargeVoltage;
        }
    }
}
