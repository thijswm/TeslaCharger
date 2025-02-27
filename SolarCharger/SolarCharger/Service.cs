using Microsoft.Extensions.Hosting;

namespace SolarCharger
{
    public class Service : IHostedService
    {
        private readonly IStateEngine _stateEngine;

        public Service(IStateEngine stateEngine)
        {
            _stateEngine = stateEngine;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //_ = _stateEngine.FireStartAsync();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
