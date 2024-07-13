using Newtonsoft.Json;

namespace TeslaCharge.Services
{
    public class HomeWizardPowerMeter : IPowerMeter
    {
        private readonly ILogger<HomeWizardPowerMeter> _log;

        public HomeWizardPowerMeter(ILogger<HomeWizardPowerMeter> log)
        {
            _log = log;
        }

        public async Task<int> GetActivePowerAsync(string address)
        {
            _log.LogInformation("Getting active power from: '{Address}'", address);

            // make a request to the HomeWizard API
            // and return the active power
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(address);

            // convert to JSON
            var json = await response.Content.ReadAsStringAsync();

            _log.LogDebug("Received JSON: {Json}", json);
            var data = JsonConvert.DeserializeObject<dynamic>(json);

            // extract the active power
            var activePower = (int)data.active_power_w;

            return activePower;
        }
    }
}
