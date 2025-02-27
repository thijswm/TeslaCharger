using Newtonsoft.Json;

namespace SolarCharger.Services.Objects
{
    public class Vehicle
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("vehicle_id")] public long VehicleId { get; set; }
        [JsonProperty("color")] public string? Color { get; set; }
        [JsonProperty("display_name")] public string? DisplayName { get; set; }
        [JsonProperty("state")] public string? State { get; set; }

        public bool IsOnline()
        {
            return State == "online";
        }

        public override string ToString()
        {
            return $"Id: {Id}, VehicleId: {VehicleId}, Color: {Color}, DisplayName: {DisplayName}, State: {State}";
        }
    }
}
