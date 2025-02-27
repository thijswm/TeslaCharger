using System.ComponentModel.DataAnnotations;

namespace SolarCharger.EF
{
    public class Settings
    {
        [Key] public Guid Id { get; set; }
        [Required] public bool Enabled { get; set; }
        [Required] public string HomeWizardAddress { get; set; }
        [Required] public TimeSpan SolarMovingAverage { get; set; }
        [Required] public int EnoughSolarWatt { get; set; }
        [Required] public int MinimumAmp { get; set; }
        [Required] public int MaximumAmp { get; set; }
        [Required] public int NumberOfPhases { get; set; }
        [Required] public TimeSpan MinimumChargeDuration { get; set; }
        [Required] public TimeSpan MinimumCurrentDuration { get; set; }

        [Required] public string TeslaFleetAddress { get; set; }
        [Required] public string TeslaCommandsAddress { get; set; }
        [Required] public string TeslaRefreshToken { get; set; }
        [Required] public string TeslaClientId { get; set; }
        [Required] public string Vin { get; set; }
        [Required]public TimeSpan PollTime { get; set; }
        [Required] public TimeSpan MinimumInitialChargeDuration { get; set; }
    }
}
