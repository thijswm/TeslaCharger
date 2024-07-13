namespace TeslaCharge
{
    public class Settings
    {
        public string? HomeWizardAddress { get; set; }
        public string? MqttTeslaMate { get; set; }
        public int EnoughSolarSeconds { get; set; } = 300;
        public int EnoughSolarWatt { get; set; } = -1000;
    }
}
