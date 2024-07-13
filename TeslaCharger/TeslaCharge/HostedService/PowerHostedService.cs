using Microsoft.Extensions.Options;
using TeslaCharge.Services;

namespace TeslaCharge.HostedService
{
    public class PowerHostedService : IHostedService
    {
        private readonly ILogger<PowerHostedService> _log;
        private readonly IPowerMeter _powerMeter;
        private readonly IPowerService _powerService;
        private readonly Settings _settings;
        private CancellationTokenSource _cts;
        private CancellationToken _ct;

        public PowerHostedService(ILogger<PowerHostedService> log, IPowerMeter powerMeter, IOptions<Settings> settings, IPowerService powerService)
        {
            _log = log;
            _powerMeter = powerMeter;
            _settings = settings.Value;
            _powerService = powerService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("PowerHostedService is starting.");

            if (string.IsNullOrEmpty(_settings.HomeWizardAddress))
            {
                throw new InvalidOperationException("HomeWizardAddress is not set.");
            }

            _cts = new CancellationTokenSource();
            _ct = _cts.Token;
            _ = Task.Run(Run, _ct);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _cts.Dispose();
            return Task.CompletedTask;
        }

        private async void Run()
        {
            while (true)
            {
                if (_ct.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var power = await _powerMeter.GetActivePowerAsync(_settings.HomeWizardAddress!);
                    _log.LogInformation("Active power: {Power}W", power);
                    if (_powerService is PowerService powerService)
                    {
                        powerService.SetCurrentPower(power);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error getting active power.");
                }

                await Task.Delay(TimeSpan.FromSeconds(20), _ct);
            }
        }
    }
}
