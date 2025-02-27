using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolarCharger.EF;
using Newtonsoft.Json;

namespace SolarCharger.Services
{
    public class HomeWizardPowerMeter : IPowerMeter
    {
        private readonly ILogger<HomeWizardPowerMeter> _log;
        private readonly ChargeContext _chargeContext;

        public HomeWizardPowerMeter(ILogger<HomeWizardPowerMeter> log, ChargeContext chargeContext)
        {
            _log = log;
            _chargeContext = chargeContext;
        }

        public async Task<Dictionary<int, int>> GetActivePowerAsync()
        {
            //"active_power_l1_w": 433.000,
            //"active_power_l2_w": -298.000,
            //"active_power_l3_w": 73.000,
            var settings = await _chargeContext.Settings.FirstOrDefaultAsync();
            if (settings == null)
            {
                throw new Exception("No settings found");
            }

            var url = $"{settings.HomeWizardAddress}/api/v1/data";
            _log.LogInformation("Getting home wizard information from url: '{Url}'", url);


            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            // convert to JSON
            var json = await response.Content.ReadAsStringAsync();

            _log.LogDebug("Received JSON: {Json}", json);
            var data = JsonConvert.DeserializeObject<dynamic>(json);

            var result = new Dictionary<int, int>
            {
                {1, (int)data!.active_power_l1_w},
                {2, (int)data!.active_power_l2_w},
                {3, (int)data!.active_power_l3_w}
            };
            return result;
        }
    }
}
