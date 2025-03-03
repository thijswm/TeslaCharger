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
    }
}
