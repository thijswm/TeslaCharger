using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SolarCharger.Controllers;
using SolarCharger.Controllers.ViewModels;

namespace SolarCharger.Services
{
    public class HubService : IHubService
    {
        private readonly ILogger<HubService> _log;

        private readonly IHubContext<SolarHub> _solarHub;

        public HubService(IHubContext<SolarHub> solarHub, ILogger<HubService> log)
        {
            _solarHub = solarHub;
            _log = log;
        }

        public async Task SendStateChangedAsync(eState state)
        {
            _log.LogInformation("Sending GUI update with state: '{State}'", state);
            var stateViewModel = StateViewModel.FromState(state);
            await _solarHub.Clients.All.SendAsync("StateChanged", stateViewModel);
        }
    }
}
