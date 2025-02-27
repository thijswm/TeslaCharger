using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SolarChargerFE.Components.Pages
{
    public partial class Settings : MudComponentBase
    {
        [Inject] public SolarChargerClient _client { get; set; }
        [Inject] private ISnackbar _snackBar { get; set; }

        private bool _loading;
        private string? _loadError;
        private SettingsViewModel _settings;

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            try
            {
                _settings = await _client.Get_settingsAsync();
            }
            catch (ApiException apiEx) when (apiEx.StatusCode == 404)
            {
                _settings = new SettingsViewModel
                {
                    HomeWizardAddress = "http://",
                    SolarMovingAverage = "00:01:00",
                    MinimumChargeDuration = "00:30:00",
                    MinimumCurrentDuration = "00:05:00",
                    MinimumInitialChargeDuration = "00:01:00",
                    PollTime = "00:00:30",
                    TeslaFleetAddress = "https://fleet-auth.prd.vn.cloud.tesla.com",
                    TeslaCommandsAddress = "https://",
                    MinimumAmp = 5,
                    MaximumAmp = 16,
                    NumberOfPhases = 3
                };
            }
            catch (Exception exp)
            {
                _loadError = $"Failed to load existing settings, Error: '{exp.Message}'";
            }
            finally
            {
                _loading = false;
            }
        }

        private TimeSpan? SolarMovingAverage
        {
            get
            {
                if (TimeSpan.TryParse(_settings.SolarMovingAverage, out var timeSpan))
                {
                    return timeSpan;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    _settings.SolarMovingAverage = value.ToString();
                }
            }
        }

        private TimeSpan? MinimumChargeDuration
        {
            get
            {
                if (TimeSpan.TryParse(_settings.MinimumChargeDuration, out var timeSpan))
                {
                    return timeSpan;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    _settings.MinimumChargeDuration = value.ToString();
                }
            }
        }

        private TimeSpan? MinimumCurrentDuration
        {
            get
            {
                if (TimeSpan.TryParse(_settings.MinimumCurrentDuration, out var timeSpan))
                {
                    return timeSpan;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    _settings.MinimumCurrentDuration = value.ToString();
                }
            }
        }

        private TimeSpan? MinimumInitialChargeDuration
        {
            get
            {
                if (TimeSpan.TryParse(_settings.MinimumInitialChargeDuration, out var timeSpan))
                {
                    return timeSpan;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    _settings.MinimumInitialChargeDuration = value.ToString();
                }
            }
        }

        private int? PollTime
        {
            get
            {
                if (TimeSpan.TryParse(_settings.PollTime, out var timeSpan))
                {
                    return (int?)timeSpan.TotalSeconds;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    _settings.PollTime = TimeSpan.FromSeconds((double)value).ToString();
                }
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                await _client.Update_settingsAsync(_settings);
                _snackBar.Add("Settings updated", Severity.Success);
            }
            catch (Exception exp)
            {
                _snackBar.Add($"Failed to update settings, Error: '{exp.Message}'", Severity.Error);
            }
        }
    }
}
