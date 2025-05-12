using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SolarCharger.Controllers;
using SolarCharger.Controllers.ViewModels;
using SolarCharger.Services.Objects;
using SolarCharger.Services.ViewModels;

namespace SolarCharger.Services
{
    public class HubService(IHubContext<SolarHub> solarHub, ILogger<HubService> log) : IHubService
    {
        public async Task SendStateChangedAsync(eState state)
        {
            log.LogInformation("Sending GUI update with state: '{State}'", state);
            var stateViewModel = StateViewModel.FromState(state);
            await solarHub.Clients.All.SendAsync("StateChanged", stateViewModel);
        }

        public Task SendVehicleDataAsync(VehicleData data)
        {
            var vehicleDataViewModel = StreamVehicleData.FromModel(data);
            return solarHub.Clients.All.SendAsync("VehicleData", vehicleDataViewModel);
        }

        public Task SendLatestPowerHistoryAsync(List<PowerHistory> powerHistory)
        {
            var powerHistoryViewModel = powerHistory
                .OrderBy(a => a.Time)
                .Select(PowerHistoryViewModel.FromModel).ToList();
            return solarHub.Clients.All.SendAsync("PowerHistory", powerHistoryViewModel);
        }

        public Task SendLoggingAsync(string logLine)
        {
            return solarHub.Clients.All.SendAsync("Logging", logLine);
        }
    }
}
