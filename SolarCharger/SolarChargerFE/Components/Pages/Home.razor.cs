using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using static MudBlazor.CategoryTypes;
using System;

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
        private List<TimeSeriesChartSeries> _powerChart = new();
        private TimeSeriesChartSeries _powerSeries = new();
        private TimeSeriesChartSeries _compensatedPowerSeries = new();

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            try
            {
                // get the initial state
                _currentState = (await _client.Get_stateAsync()).State;

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

                _powerChart.Add(_powerSeries);
                _powerChart.Add(_compensatedPowerSeries);

                _hubConnection.On<List<PowerHistory>>("PowerHistory", (history) =>
                {
                    _powerSeries.Data = history
                        .Select(a => new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.Power)).ToList();

                    _compensatedPowerSeries.Data = history
                        .Where(a => a.CompensatedPower.HasValue)
                        .Select(a => new TimeSeriesChartSeries.TimeValue(a.Time.LocalDateTime, a.CompensatedPower!.Value)).ToList();
                    InvokeAsync(StateHasChanged);
                });

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
