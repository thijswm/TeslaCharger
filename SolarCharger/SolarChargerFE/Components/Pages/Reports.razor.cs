using Microsoft.AspNetCore.Components;
using MudBlazor;
using SolarChargerFE.Components.Pages.ViewModel;

namespace SolarChargerFE.Components.Pages
{
    public partial class Reports : MudComponentBase
    {
        [Inject] public SolarChargerClient _client { get; set; }
        [Inject] private ISnackbar _snackBar { get; set; }
        private bool _loading;
        private string? _loadError;
        private List<ChargeSession> _chargeSessions = new();
        private ChargeSession? _selectedChargeSession;

        private bool _loadingChargeSession;
        private readonly List<TimeSeriesChartSeries> _powerChart = new();
        private TimeSeriesChartSeries _powerSeries = new();
        private TimeSeriesChartSeries _compensatedPowerSeries = new();

        protected override async Task OnInitializedAsync()
        {
            _loading = true;

            _powerSeries = new TimeSeriesChartSeries
            {
                Index = 0,
                Name = "Power",
                IsVisible = true,
                Type = TimeSeriesDisplayType.Line
            };

            _compensatedPowerSeries = new TimeSeriesChartSeries
            {
                Index = 0,
                Name = "Compensated Power",
                IsVisible = true,
                Type = TimeSeriesDisplayType.Line
            };

            _powerChart.Add(_powerSeries);
            _powerChart.Add(_compensatedPowerSeries);


            try
            {
                var sessions = await _client.Get_charge_sessionsAsync();
                _chargeSessions = sessions.Select(a => new ChargeSession(a)).OrderByDescending(a => a.StartDate)
                    .ToList();
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

        private async void OnChargeSessionClick(DataGridRowClickEventArgs<ChargeSession> chargeSession)
        {
            _selectedChargeSession = chargeSession.Item;

            _loadingChargeSession = true;
            if (_selectedChargeSession != null)
            {
                try
                {
                    var powerHistory = await _client.Get_power_history_for_sessionAsync(_selectedChargeSession.Id);


                    _powerSeries.Data = powerHistory
                        .Select(a => new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.Power)).ToList();

                    _compensatedPowerSeries.Data = powerHistory
                        .Where(a => a.CompensatedPower.HasValue)
                        .Select(a => new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.CompensatedPower!.Value)).ToList();

                    await InvokeAsync(StateHasChanged);
                }
                catch (Exception exp)
                {
                    _snackBar.Add($"Failed to load power history for session, Error: '{exp.Message}'", Severity.Error);
                }
            }

            _loadingChargeSession = false;
            StateHasChanged();
        }
    }
}
