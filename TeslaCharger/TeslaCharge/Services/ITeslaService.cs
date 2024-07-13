namespace TeslaCharge.Services
{
    public class TeslaData
    {
        public int? BatteryLevel { get; set; }
        public string? State { get; set; }
        public bool? PluggedIn { get; set; }
        public int? ChargeLimitSoc { get; set; }
    }


    public interface ITeslaService
    {
        event Action<TeslaData>? OnDataChanged;
        TeslaData LatestData { get; }
    }
}
