using Microsoft.Extensions.Hosting;
using SolarCharger.Services;

namespace SolarCharger
{
    public class Service(IStateEngine stateEngine, IHubService hubService) : IHostedService
    {
        private readonly IStateEngine _stateEngine = stateEngine;
        private Task? _backgroundTask;
        private CancellationTokenSource? _cts;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _backgroundTask = Run(_cts.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_cts != null)
            {
                await _cts.CancelAsync();
                if (_backgroundTask != null)
                {
                    await Task.WhenAny(_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
                }
            }
        }

        private async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                while (LogCollectorProvider.LogQueue.TryDequeue(out var log))
                {
                    await hubService.SendLoggingAsync(log);
                }
                await Task.Delay(500, token);
            }
        }
    }
}
