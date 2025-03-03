using SolarCharger.Services.Objects;

namespace SolarCharger.Services.ViewModels
{
    public class StreamVehicleData
    {
        public int BatteryLevel { get; set; }
        public int ChargeAmps { get; set; }
        public int ChargeCurrentRequest { get; set; }
        public int ChargeCurrentRequestMax { get; set; }
        public string ChargePortLatch { get; set; } = string.Empty;
        public int ChargerPower { get; set; }
        public int ChargerVoltage { get; set; }
        public double ChargeEnergyAdded { get; set; }
        public int ChargeLimitSoc { get; set; }

        public static StreamVehicleData FromModel(VehicleData model)
        {
            return new StreamVehicleData
            {
                BatteryLevel = model.ChargeState?.BatteryLevel ?? 0,
                ChargeAmps = model.ChargeState?.ChargeAmps ?? 0,
                ChargeCurrentRequest = model.ChargeState?.ChargeCurrentRequest ?? 0,
                ChargeCurrentRequestMax = model.ChargeState?.ChargeCurrentRequestMax ?? 0,
                ChargePortLatch = model.ChargeState?.ChargePortLatch ?? string.Empty,
                ChargerPower = model.ChargeState?.ChargerPower ?? 0,
                ChargerVoltage = model.ChargeState?.ChargerVoltage ?? 0,
                ChargeEnergyAdded = model.ChargeState?.ChargeEnergyAdded ?? 0,
                ChargeLimitSoc = model.ChargeState?.ChargeLimitSoc ?? 0
            };
        }
    }
}
