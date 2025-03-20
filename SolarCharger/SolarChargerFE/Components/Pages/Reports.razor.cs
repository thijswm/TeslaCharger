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
        private ChartOptions _powerOptions = new();
        private readonly List<TimeSeriesChartSeries> _powerChart = new();
        private TimeSeriesChartSeries _powerSeries = new();
        private TimeSeriesChartSeries _compensatedPowerSeries = new();

        private ChartOptions _currentOptions = new();
        private readonly List<TimeSeriesChartSeries> _currentChart = new();
        private TimeSeriesChartSeries _currentSeries = new();

        private readonly List<TimeSeriesChartSeries> _groupedData = new();
        private TimeSeriesChartSeries _kwhCompensatedSeries = new();

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            _powerOptions.XAxisLines = true;
            _currentOptions.XAxisLines = true;
            _currentOptions.YAxisTicks = 1;
            _currentOptions.YAxisRequireZeroPoint = true;

            _powerSeries = new TimeSeriesChartSeries
            {
                Index = 0,
                Name = "Power",
                IsVisible = true,
                LineDisplayType = LineDisplayType.Line
            };

            _compensatedPowerSeries = new TimeSeriesChartSeries
            {
                Index = 1,
                Name = "Compensated Power",
                IsVisible = true,
                LineDisplayType = LineDisplayType.Line
            };

            _currentSeries = new TimeSeriesChartSeries
            {
                Index = 2,
                Name = "Current",
                IsVisible = true,
                LineDisplayType = LineDisplayType.Line
            };

            _kwhCompensatedSeries = new TimeSeriesChartSeries
            {
                Index = 0,
                Name = "kWh",
                IsVisible = true,
                LineDisplayType = LineDisplayType.Area
            };

            _powerChart.Add(_powerSeries);
            _powerChart.Add(_compensatedPowerSeries);
            _currentChart.Add(_currentSeries);
            _groupedData.Add(_kwhCompensatedSeries);

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

        private ChargeSession? SelectedChargeSession
        {
            get => _selectedChargeSession;
            set
            {
                if (value != null)
                {
                    OnChargeSessionClick(value);
                    _selectedChargeSession = value;
                }
            }
        }

        private void OnChargeSessionClick(ChargeSession chargeSession)
        {
            _selectedChargeSession = chargeSession;

            Task.Run(async () =>
            {
                _loadingChargeSession = true;
                await InvokeAsync(StateHasChanged);
                if (_selectedChargeSession != null)
                {
                    try
                    {
                        var powerHistory = await _client.Get_power_history_for_sessionAsync(_selectedChargeSession.Id);

                        _powerSeries.Data = powerHistory
                            .Select(a => new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.Power)).ToList();

                        _compensatedPowerSeries.Data = powerHistory
                            .Where(a => a.CompensatedPower.HasValue)
                            .Select(a =>
                                new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.CompensatedPower!.Value))
                            .ToList();


                        var currentChanges =
                            await _client.Get_current_changes_for_sessionAsync(_selectedChargeSession.Id);
                        _currentSeries.Data = currentChanges
                            .Select(a => new TimeSeriesChartSeries.TimeValue(a.Timestamp.LocalDateTime, a.Current))
                            .ToList();

                        var compensatedGrouped = powerHistory
                            .Where(a => a.CompensatedPower.HasValue)
                            .Select(a => new
                            {
                                HourDate = a.Time.Date.AddHours(a.Time.Hour),
                                TimeStamp = a.Time,
                                CompensatedPower = a.CompensatedPower!.Value
                            })
                            .GroupBy(a => a.HourDate)
                            .Select(a =>
                            {
                                var allTimes = a.Select(a => a.TimeStamp).ToList();
                                var averageSeconds = CalculateAverageSecondsBetweenTimestamps(allTimes);
                                var dt = averageSeconds / 3600.0;
                                var energy = a.Sum(b =>
                                {
                                    var val = b.CompensatedPower;
                                    if (val < 0)
                                    {
                                        val *= -1;
                                    }

                                    return val * dt / 1000;
                                });
                                return new
                                {
                                    HourDate = a.Key,
                                    Energy = energy,
                                    AverageSeconds = averageSeconds
                                };
                            });

                        _kwhCompensatedSeries.Data = compensatedGrouped.Select(a => new TimeSeriesChartSeries.TimeValue(a.HourDate, a.Energy)).ToList();
                    }
                    catch (Exception exp)
                    {
                        _snackBar.Add($"Failed to load power history for session, Error: '{exp.Message}'",
                            Severity.Error);
                    }
                }
                _loadingChargeSession = false;
                await InvokeAsync(StateHasChanged);
            });
        }

        static double CalculateAverageSecondsBetweenTimestamps(List<DateTimeOffset> timestamps)
        {
            if (timestamps.Count < 2)
                return 0; // Not enough timestamps to calculate differences

            // Sort timestamps
            timestamps = timestamps.OrderBy(t => t).ToList();

            // Calculate differences in seconds
            var differences = new List<double>();
            for (var i = 1; i < timestamps.Count; i++)
            {
                var diff = (timestamps[i] - timestamps[i - 1]).TotalSeconds;
                differences.Add(diff);
            }

            // Return the average difference
            return differences.Average();
        }
    }
}
