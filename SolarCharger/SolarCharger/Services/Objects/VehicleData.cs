using Newtonsoft.Json;

namespace SolarCharger.Services.Objects
{
    public class VehicleDataChargeState
    {
        [JsonProperty("battery_level")] public int BatteryLevel { get; set; }
        [JsonProperty("charge_amps")] public int ChargeAmps { get; set; }
        [JsonProperty("charge_current_request")] public int ChargeCurrentRequest { get; set; }
        [JsonProperty("charge_current_request_max")] public int ChargeCurrentRequestMax { get; set; }
        [JsonProperty("charge_port_latch")] public string ChargePortLatch { get; set; } = string.Empty;
        [JsonProperty("charger_power")] public int ChargerPower { get; set; }
        [JsonProperty("charger_voltage")] public int ChargerVoltage { get; set; }
        [JsonProperty("charge_energy_added")] public double ChargeEnergyAdded { get; set; }
        [JsonProperty("charge_limit_soc")] public int ChargeLimitSoc { get; set; }

        [JsonIgnore] public bool IsChargePortLatched => ChargePortLatch == "Engaged";
        [JsonIgnore] public int ChargePowerWatt => ChargerPower * 1000;

        public override string ToString()
        {
            return $"BatteryLevel: {BatteryLevel}% ChargeAmps: {ChargeAmps}A {ChargeCurrentRequest}A/{ChargeCurrentRequestMax}A ChargePortLatch: '{ChargePortLatch}' ChargerPower: {ChargerPower} KW ChargerVoltage: {ChargerVoltage} V ChargeLimitSoc: {ChargeLimitSoc}";
        }
    }

    public class VehicleData
    {
        [JsonProperty("charge_state")] public VehicleDataChargeState? ChargeState { get; set; }

        public override string ToString()
        {
            return ChargeState?.ToString() ?? "No charge state";
        }
    }
}
