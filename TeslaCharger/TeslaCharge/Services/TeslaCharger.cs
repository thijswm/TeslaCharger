using Microsoft.Extensions.Options;

namespace TeslaCharge.Services
{
    public class TeslaCharger : ITeslaCharger
    {
        private readonly ILogger<TeslaCharger> _log;
        private readonly Settings _settings;
        private readonly List<KeyValuePair<DateTime, int>> _powerHistory = new();
        private State _currentState;


        public TeslaCharger(ITeslaService teslaService, IPowerService powerService, ILogger<TeslaCharger> log, IOptions<Settings> settings)
        {
            _log = log;
            _settings = settings.Value;
            _currentState = State.NotPluggedIn;
            teslaService.OnDataChanged += TeslaService_OnDataChanged;
            powerService.OnPowerChanged += PowerService_OnPowerChanged;

            TeslaService_OnDataChanged(teslaService.LatestData);
        }

        private void PowerService_OnPowerChanged(int power)
        {
            switch (CurrentState)
            {
                case State.PluggedIn:
                case State.NotEnoughSolar:
                    CheckSolarPower(power);
                    break;
            }
        }

        private void CheckSolarPower(int power)
        {
            // we are plugged in, we will monitor average power use last N minutes
            _powerHistory.Add(new KeyValuePair<DateTime, int>(DateTime.Now, power));

            // get the oldest entry
            var oldest = _powerHistory.OrderBy(x => x.Key).FirstOrDefault();
            if (oldest.Key <= DateTime.Now.AddSeconds(_settings.EnoughSolarSeconds * -1))
            {
                // oldest is at least N minutes old
                // calculate average power use
                var average = _powerHistory.Average(x => x.Value);
                _log.LogInformation("Average power use: {Average}W", average);

                if (average <= _settings.EnoughSolarWatt)
                {
                    // enough solar power, we can charge
                    _log.LogInformation("Enough solar power, we can charge");
                    _currentState = State.StartCharging;
                }
                else
                {
                    // not enough
                    _currentState = State.NotEnoughSolar;
                }
            }
            else
            {
                var delta = DateTime.Now.AddSeconds(_settings.EnoughSolarSeconds * -1) - oldest.Key;
                _log.LogInformation("Not enough data to calculate average power use, Oldest is: {DateTime} Need another: {Seconds} seconds", oldest.Key, ((int)delta.TotalSeconds) * -1);
                _currentState = State.NotEnoughSolar;
            }

            // remove all entries older than 5 minutes
            _powerHistory.RemoveAll(x => x.Key < DateTime.Now.AddSeconds(_settings.EnoughSolarSeconds * -1));
        }

        private void TeslaService_OnDataChanged(TeslaData data)
        {
            if (data.PluggedIn.GetValueOrDefault(false) && CurrentState == State.NotPluggedIn)
            {
                _log.LogInformation("Tesla state changed to plugged-in");
                _currentState = State.PluggedIn;
            }
            else if (!data.PluggedIn.GetValueOrDefault(false))
            {
                _log.LogInformation("Tesla state changed to not plugged-in");
                _currentState = State.NotPluggedIn;
            }
        }

        public event Action<State>? OnStateChanged;

        public State CurrentState
        {
            get => _currentState;

            set
            {
                _currentState = value;
                OnStateChanged?.Invoke(CurrentState);
            }
        }
    }
}
