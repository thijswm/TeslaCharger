using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace SolarChargerFE.Components.Pages
{
    public partial class Home : MudComponentBase
    {
        [Inject] public HubConnection _hubConnection { get; set; }
        [Inject] public SolarChargerClient _client { get; set; }
        [Inject] private ISnackbar _snackBar { get; set; }
        private bool _loading;
        private string _loadError;
        private EState _currentState;
        private StreamVehicleData? _currentVehicleData;
        private readonly List<TimeSeriesChartSeries> _powerChart = new();
        private TimeSeriesChartSeries _powerSeries = new();
        private TimeSeriesChartSeries _compensatedPowerSeries = new();

        private readonly List<TimeSeriesChartSeries> _currentChart = new();
        private TimeSeriesChartSeries _currentSeries = new();


        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            try
            {
                // get the initial state
                _hubConnection.On<StateViewModel>("StateChanged", (message) =>
                {
                    _currentState = message.State;
                    InvokeAsync(StateHasChanged);
                });

                _hubConnection.On<StreamVehicleData>("VehicleData", (vehicleData) =>
                {
                    _currentVehicleData = vehicleData;
                    InvokeAsync(StateHasChanged);
                });

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

                _currentSeries = new TimeSeriesChartSeries
                {
                    Index = 0,
                    Name = "Current",
                    IsVisible = true,
                    Type = TimeSeriesDisplayType.Line
                };

                _powerChart.Add(_powerSeries);
                _powerChart.Add(_compensatedPowerSeries);
                _currentChart.Add(_currentSeries);

                _hubConnection.On<List<PowerHistory>>("PowerHistory", (history) =>
                {
                    _powerSeries.Data = history
                        .Select(a => new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.Power)).ToList();

                    _compensatedPowerSeries.Data = history
                        .Where(a => a.CompensatedPower.HasValue)
                        .Select(a => new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.CompensatedPower!.Value)).ToList();
                    InvokeAsync(StateHasChanged);
                });

                _currentState = (await _client.Get_stateAsync()).State;

                if (_currentState != EState.Idle)
                {
                    _currentVehicleData = await _client.Get_latest_vehicle_dataAsync();
                    var history = await _client.Get_power_historyAsync();

                    _powerSeries.Data = history
                        .Select(a => new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.Power)).ToList();

                    _compensatedPowerSeries.Data = history
                        .Where(a => a.CompensatedPower.HasValue)
                        .Select(a => new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.CompensatedPower!.Value)).ToList();
                }

                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                }
            }
            catch (Exception exp)
            {
                _loadError = $"Failed to connect with the backend, Error: '{exp.Message}'";
            }
            _loading = false;
        }


        private async Task StartChargingAsync()
        {
            try
            {
                await _client.Start_chargeAsync();
                _snackBar.Add("Charging session started", Severity.Success);
            }
            catch (Exception exp)
            {
                _snackBar.Add($"Failed to start charging, Error: '{exp.Message}'", Severity.Error);
            }
        }

        private async Task StopChargingAsync()
        {
            try
            {
                await _client.Stop_chargeAsync();
                _snackBar.Add("Charging session is stopping", Severity.Success);
            }
            catch (Exception exp)
            {
                _snackBar.Add($"Failed to stop charging, Error: '{exp.Message}'", Severity.Error);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
