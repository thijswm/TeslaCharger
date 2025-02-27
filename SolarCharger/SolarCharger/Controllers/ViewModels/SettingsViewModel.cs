using System.ComponentModel.DataAnnotations;
using SolarCharger.EF;

namespace SolarCharger.Controllers.ViewModels
{
    public class SettingsViewModel
    {
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
        [Required] public TimeSpan PollTime { get; set; }
        [Required] public TimeSpan MinimumInitialChargeDuration { get; set; }

        public Settings ToModel()
        {
            return new Settings
            {
                Id = Guid.NewGuid(),
                Enabled = Enabled,
                HomeWizardAddress = HomeWizardAddress,
                SolarMovingAverage = SolarMovingAverage,
                EnoughSolarWatt = EnoughSolarWatt,
                MinimumAmp = MinimumAmp,
                MaximumAmp = MaximumAmp,
                NumberOfPhases = NumberOfPhases,
                MinimumChargeDuration = MinimumChargeDuration,
                MinimumCurrentDuration = MinimumCurrentDuration,
                TeslaFleetAddress = TeslaFleetAddress,
                TeslaCommandsAddress = TeslaCommandsAddress,
                TeslaRefreshToken = TeslaRefreshToken,
                TeslaClientId = TeslaClientId,
                Vin = Vin,
                PollTime = PollTime,
                MinimumInitialChargeDuration = MinimumInitialChargeDuration
            };
        }

        public static SettingsViewModel FromModel(Settings model)
        {
            return new SettingsViewModel
            {
                Enabled = model.Enabled,
                HomeWizardAddress = model.HomeWizardAddress,
                SolarMovingAverage = model.SolarMovingAverage,
                EnoughSolarWatt = model.EnoughSolarWatt,
                MinimumAmp = model.MinimumAmp,
                MaximumAmp = model.MaximumAmp,
                NumberOfPhases = model.NumberOfPhases,
                MinimumChargeDuration = model.MinimumChargeDuration,
                MinimumCurrentDuration = model.MinimumCurrentDuration,
                TeslaFleetAddress = model.TeslaFleetAddress,
                TeslaCommandsAddress = model.TeslaCommandsAddress,
                TeslaRefreshToken = model.TeslaRefreshToken,
                TeslaClientId = model.TeslaClientId,
                Vin = model.Vin,
                PollTime = model.PollTime,
                MinimumInitialChargeDuration = model.MinimumInitialChargeDuration
            };
        }
    }
}
