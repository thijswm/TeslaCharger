using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolarCharger.EF;
using SolarCharger.Services;
using SolarCharger.Services.Objects;
using Stateless;

namespace SolarCharger
{
    public class PowerAverage
    {
        public bool TimeReached { get; set; }
        public int? Value { get; set; }
        public TimeSpan? TimeLeft { get; set; }
    }




    public class StateEngine : IStateEngine
    {
        private readonly ILogger<StateEngine> _log;

        private readonly StateMachine<eState, eTrigger> _stateMachine;
        private readonly IPowerMeter _powerMeter;
        private readonly ITesla _tesla;
        private readonly IHubService _hubService;
        private readonly ChargeContext _chargeContext;
        private readonly ChargeSessionService _chargeSessionService;
        private Settings? _settings;
        private readonly List<KeyValuePair<DateTime, int>> _powerHistory = new();
        private readonly List<KeyValuePair<DateTime, int>> _compensatedHistory = new();
        private readonly List<PowerHistory> _totalPowerHistory = new();
        private int _numPhases = 3;
        private DateTime _lastChargeStart = DateTime.MinValue;
        private DateTime _lastChargeMonitoringStart = DateTime.MinValue;
        private bool _stopRequested;
        private eState _state = eState.Idle;
        private StateMachine<eState, eTrigger>.TriggerWithParameters<int>? _startChargeTrigger;
        private ChargeSession? _currentChargeSession;

        public StateEngine(ILogger<StateEngine> log, IHubService hubService, ChargeContext chargeContext, IPowerMeter powerMeter, ITesla tesla, ChargeSessionService chargeSessionService)
        {
            _log = log;
            _hubService = hubService;
            _chargeContext = chargeContext;
            _powerMeter = powerMeter;
            _tesla = tesla;
            _chargeSessionService = chargeSessionService;
            _stateMachine = new StateMachine<eState, eTrigger>(() => State, s => State = s);

            _startChargeTrigger = _stateMachine.SetTriggerParameters<int>(eTrigger.StartCharge);

            _stateMachine.Configure(eState.Idle)
                .Permit(eTrigger.Start, eState.Starting);

            _stateMachine.Configure(eState.Starting)
                .OnEntryAsync(OnStartAsync)
                .Permit(eTrigger.Started, eState.IsTeslaOnline);

            _stateMachine.Configure(eState.IsTeslaOnline)
                .OnEntryAsync(OnCheckIsTeslaOnline)
                .PermitReentry(eTrigger.TeslaCheckOnline)
                .Permit(eTrigger.TeslaOnline, eState.TeslaOnline)
                .Permit(eTrigger.StopChargeDone, eState.Idle);

            _stateMachine.Configure(eState.TeslaOnline)
                .OnEntryAsync(OnTeslaOnline)
                .PermitReentry(eTrigger.TeslaOnline)
                .Permit(eTrigger.TeslaCanCharge, eState.CheckSolarPower)
                .Permit(eTrigger.Error, eState.IsTeslaOnline)
                .Permit(eTrigger.StopChargeDone, eState.Idle);

            _stateMachine.Configure(eState.CheckSolarPower)
                .OnEntryAsync(OnCheckSolarPowerAsync)
                .PermitReentry(eTrigger.TeslaCanCharge)
                .Permit(eTrigger.EnoughSolarPower, eState.EnoughSolarPower)
                .Permit(eTrigger.StopChargeDone, eState.Idle);

            _stateMachine.Configure(eState.EnoughSolarPower)
                .OnEntryAsync(OnEnoughSolarPowerAsync)
                .Permit(eTrigger.StartCharge, eState.StartCharge);

            _stateMachine.Configure(eState.StartCharge)
                .OnEntryFromAsync(_startChargeTrigger, OnStartChargeAsync)
                .Permit(eTrigger.Charging, eState.InitialCharging);

            _stateMachine.Configure(eState.InitialCharging)
                .OnEntryAsync(OnInitialChargingAsync)
                .PermitReentry(eTrigger.Charging)
                .Permit(eTrigger.InitialChargeDurationReached, eState.InitialChargeDurationReached)
                .Permit(eTrigger.StopCharge, eState.StopCharge);

            _stateMachine.Configure(eState.InitialChargeDurationReached)
                .OnEntryAsync(OnInitialChargeDurationReachedAsync)
                .Permit(eTrigger.MonitorCharge, eState.MonitoringCharge)
                .Permit(eTrigger.Error, eState.IsTeslaOnline);

            _stateMachine.Configure(eState.MonitoringCharge)
                .OnEntryAsync(OnMonitoringCharge)
                .PermitReentry(eTrigger.MonitorCharge)
                .Permit(eTrigger.MonitorChargeDurationReached, eState.MonitoringChargeDurationReached)
                .Permit(eTrigger.StopCharge, eState.StopCharge);

            _stateMachine.Configure(eState.MonitoringChargeDurationReached)
                .OnEntryAsync(OnMonitorChargeDurationReachedAsync)
                .Permit(eTrigger.MonitorCharge, eState.MonitoringCharge)
                .Permit(eTrigger.StopCharge, eState.StopCharge)
                .Permit(eTrigger.Error, eState.IsTeslaOnline);

            _stateMachine.Configure(eState.StopCharge)
                .OnEntryAsync(OnStopChargeAsync)
                .Permit(eTrigger.StopChargeDone, eState.Idle);

            _stateMachine.OnTransitioned(StateTransitioned);
        }

        public eState State
        {
            get => _state;
            set => _state = value;
        }

        public VehicleData? LatestVehicleData { get; private set; }

        public List<PowerHistory> GetPowerHistory()
        {
            return _totalPowerHistory;
        }

        private async void StateTransitioned(StateMachine<eState, eTrigger>.Transition transition)
        {
            _log.LogDebug(
                "Trigger: '{Trigger}' fired from State: '{Source}' to State: '{Destination}'", transition.Trigger,
                transition.Source, transition.Destination);

            await _hubService.SendStateChangedAsync(transition.Destination);
        }

        private async Task AddPowerAsync(int power, int? compensatedPower)
        {
            var powerHistory = new PowerHistory
            {
                Time = DateTime.Now,
                Power = power,
                CompensatedPower = compensatedPower,
            };

            _totalPowerHistory.Add(powerHistory);

            if (_currentChargeSession != null)
            {
                await _chargeSessionService.AddPowerHistoryAsync(new ChargePower
                {
                    Id = Guid.NewGuid(),
                    ChargeSessionId = _currentChargeSession.Id,
                    ChargeSession = _currentChargeSession,
                    Timestamp = powerHistory.Time,
                    Power = power,
                    CompensatedPower = compensatedPower
                });

                await _hubService.SendLatestPowerHistoryAsync(_totalPowerHistory);
            }
        }

        public async Task FireStartAsync()
        {
            _log.LogInformation("Starting State Engine");
            if (_stateMachine.CanFire(eTrigger.Start))
            {
                await _stateMachine.FireAsync(eTrigger.Start);
            }
        }

        public Task FireStopAsync()
        {
            _log.LogInformation("Stopping State Engine");
            _stopRequested = true;
            return Task.CompletedTask;
        }

        private async Task OnStartAsync()
        {
            _stopRequested = false;

            _powerHistory.Clear();
            _compensatedHistory.Clear();
            _totalPowerHistory.Clear();

            // do some startup check stuff
            // for now skip
            _settings = await _chargeContext.Settings.FirstOrDefaultAsync();
            if (_settings == null)
            {
                _log.LogError("No settings found in database");
            }
            else if (!_settings.Enabled)
            {
                _log.LogError("Charging not enabled");
            }
            else
            {
                try
                {
                    await _tesla.StartAsync();

                    await _stateMachine.FireAsync(eTrigger.Started);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error starting Tesla");
                }
            }
        }

        private async Task OnCheckIsTeslaOnline()
        {
            if (_stopRequested)
            {
                await _stateMachine.FireAsync(eTrigger.StopChargeDone);
            }
            else
            {
                try
                {
                    var isOnline = await _tesla.IsOnlineAsync();
                    if (isOnline)
                    {
                        await _stateMachine.FireAsync(eTrigger.TeslaOnline);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError("Failed to check if tesla is online, Error: '{Error}'", ex.Message);
                }

                _log.LogDebug("Waiting for {Seconds} sec.", _settings!.PollTime.TotalSeconds);
                await Task.Delay(_settings.PollTime);
                await _stateMachine.FireAsync(eTrigger.TeslaCheckOnline);
            }
        }

        private async Task OnTeslaOnline()
        {
            if (_stopRequested)
            {
                await _stateMachine.FireAsync(eTrigger.StopChargeDone);
            }
            else
            {
                // we now once check if the charger port is latched
                try
                {
                    _log.LogInformation("Getting vehicle data");
                    LatestVehicleData = await _tesla.GetVehicleDataAsync();
                    _log.LogInformation("Vehicle data: {Data}", LatestVehicleData);

                    await _hubService.SendVehicleDataAsync(LatestVehicleData);

                    if (LatestVehicleData.ChargeState is { IsChargePortLatched: true })
                    {
                        await _stateMachine.FireAsync(eTrigger.TeslaCanCharge);
                        return;
                    }
                }
                catch (Exception exp)
                {
                    _log.LogError("Failed to get vehicle data, Error: '{Error}'", exp.Message);
                }

                _powerHistory.Clear();
                _compensatedHistory.Clear();
                _totalPowerHistory.Clear();

                _log.LogDebug("Waiting for {Seconds} sec.", _settings!.PollTime.TotalSeconds);
                await Task.Delay(_settings.PollTime);
                await _stateMachine.FireAsync(eTrigger.TeslaOnline);
            }
        }

        private async Task OnCheckSolarPowerAsync()
        {
            if (_stopRequested)
            {
                await _stateMachine.FireAsync(eTrigger.StopChargeDone);
            }
            else
            {
                // always add the latest power
                var currentPower = 0;
                var validPower = false;
                try
                {
                    var activePower = await _powerMeter.GetActivePowerAsync();
                    _numPhases = activePower.Count; // remember the number of phases
                    currentPower = activePower.Values.Sum();
                    _powerHistory.Add(
                        new KeyValuePair<DateTime, int>(DateTime.Now, currentPower));
                    validPower = true;

                    await AddPowerAsync(currentPower, null);
                }
                catch (Exception ex)
                {
                    _log.LogError("Failed to get active power, Error: '{Error}'", ex.Message);
                }


                var enoughSolar = false;
                if (validPower)
                {
                    var movingAverage = GetCurrentMovingAverage();
                    if (movingAverage.TimeReached)
                    {
                        _log.LogInformation(
                            "Current power: {CurrentPower}, Moving average: {MovingAverage} Needs to be below: {Below} W",
                            currentPower, movingAverage.Value, _settings!.EnoughSolarWatt);
                        if (movingAverage.Value <= _settings!.EnoughSolarWatt)
                        {
                            enoughSolar = true;
                            await _stateMachine.FireAsync(eTrigger.EnoughSolarPower);
                        }
                    }
                    else
                    {
                        _log.LogInformation(
                            "Current power: {CurrentPower}, Moving average: {MovingAverage} Needs to be below: {Below} W, Time Left: {TimeLeft} sec.",
                            currentPower, movingAverage.Value, _settings!.EnoughSolarWatt,
                            (int?)movingAverage.TimeLeft?.TotalSeconds);
                    }
                }

                if (!enoughSolar)
                {
                    _log.LogDebug("Waiting for {Seconds} sec.", _settings!.PollTime.TotalSeconds);
                    await Task.Delay(_settings.PollTime);
                    await _stateMachine.FireAsync(eTrigger.TeslaCanCharge);
                }
            }
        }

        private PowerAverage GetCurrentMovingAverage()
        {
            var movingAverage = _powerHistory.Average(a => a.Value);
            var oldest = _powerHistory.OrderBy(x => x.Key).First();
            var startTime = DateTime.Now.AddSeconds(_settings!.SolarMovingAverage.TotalSeconds * -1);
            if (oldest.Key <= startTime)
            {
                return new PowerAverage { TimeReached = true, Value = (int?)movingAverage };
            }

            return new PowerAverage
            { TimeReached = false, Value = (int?)movingAverage, TimeLeft = oldest.Key - startTime };
        }

        private PowerAverage GetCompensatedMovingAverage(DateTime fromDateTime, TimeSpan duration)
        {
            var movingAverage = _compensatedHistory.Average(a => a.Value);
            var oldest = _compensatedHistory.OrderBy(x => x.Key).First();
            var startTime = DateTime.Now.AddSeconds(duration.TotalSeconds * -1);
            if (oldest.Key <= startTime)
            {
                return new PowerAverage { TimeReached = true, Value = (int?)movingAverage };
            }

            return new PowerAverage
            { TimeReached = false, Value = (int?)movingAverage, TimeLeft = oldest.Key - startTime };
        }

        private int GetAvailableCurrent(int availablePower)
        {
            // this happens when solar excess is higher than 0
            if (availablePower < 0)
            {
                availablePower = 0;
            }

            var possibleAmps = availablePower / (_numPhases * _tesla.CurrentChargeVoltage);

            if (possibleAmps < _settings!.MinimumAmp)
            {
                possibleAmps = _settings.MinimumAmp;
            }

            if (possibleAmps > _settings.MaximumAmp)
            {
                possibleAmps = _settings.MaximumAmp;
            }

            return possibleAmps;
        }

        private async Task OnEnoughSolarPowerAsync()
        {
            var currentAveragePower = GetCurrentMovingAverage();
            if (currentAveragePower.TimeReached)
            {
                var availablePower = currentAveragePower.Value! * -1;
                var possibleAmps = GetAvailableCurrent((int)availablePower);
                _log.LogInformation("Available power: {Power} W and Amps set to: {Amps} A", availablePower,
                    possibleAmps);

                // we do a short wait to make sure we can do a get power call again
                _log.LogDebug("Waiting for {Seconds} sec.", _settings!.PollTime.TotalSeconds);
                await Task.Delay(_settings.PollTime);

                await _stateMachine.FireAsync(_startChargeTrigger!, possibleAmps!);
            }
        }

        private async Task OnStartChargeAsync(int amps)
        {
            bool ok;

            try
            {
                await _tesla.SetChargeAmpsAsync(amps);
                ok = true;
            }
            catch (Exception ex)
            {
                _log.LogError("Failed to set charge amps, Error: '{Error}'", ex.Message);
                await _stateMachine.FireAsync(eTrigger.Error);
                return;
            }

            if (ok)
            {
                try
                {
                    await _tesla.StartChargeAsync();
                    _lastChargeStart = DateTime.Now;

                    _currentChargeSession = new ChargeSession
                    {
                        Id = Guid.NewGuid(),
                        Start = _lastChargeStart,
                        BatteryLevelStarted = _tesla.CurrentBatteryLevel
                    };
                    await _chargeSessionService.AddChargeSessionAsync(_currentChargeSession);
                }
                catch (Exception ex)
                {
                    _log.LogError("Failed to start charging, Error: '{Error}'", ex.Message);
                    await _stateMachine.FireAsync(eTrigger.Error);
                    ok = false;
                }
            }

            if (ok)
            {
                try
                {
                    if (_currentChargeSession != null)
                    {
                        var currentChange = new ChargeCurrentChange
                        {
                            Id = Guid.NewGuid(),
                            ChargeSessionId = _currentChargeSession.Id,
                            ChargeSession = _currentChargeSession,
                            Timestamp = DateTime.Now,
                            Current = amps
                        };
                        await _chargeSessionService.AddCurrentChangeAsync(currentChange);
                    }
                }
                catch (Exception exp)
                {
                    _log.LogError("Failed to add new current change, Error: '{Error}'", exp.Message);
                }

                await _stateMachine.FireAsync(eTrigger.Charging);
            }
        }

        private async Task OnInitialChargingAsync()
        {
            if (_stopRequested)
            {
                await _stateMachine.FireAsync(eTrigger.StopCharge);
            }
            else
            {
                // we keep on monitoring for a short while after charging is started
                // then we switch to the adjust charging state
                var validPower = false;
                var currentPower = 0;
                try
                {
                    var activePower = await _powerMeter.GetActivePowerAsync();
                    currentPower = activePower.Values.Sum();

                    _powerHistory.Add(
                        new KeyValuePair<DateTime, int>(DateTime.Now, currentPower));

                    await AddPowerAsync(currentPower, null);
                    validPower = true;
                }
                catch (Exception ex)
                {
                    _log.LogError("Failed to get active power, Error: '{Error}'", ex.Message);
                }

                if (validPower)
                {
                    var timeSinceChargeStarted = DateTime.Now - _lastChargeStart!;
                    if (timeSinceChargeStarted >= _settings!.MinimumInitialChargeDuration)
                    {
                        await _stateMachine.FireAsync(eTrigger.InitialChargeDurationReached);
                        return;
                    }

                    _log.LogInformation(
                        "Current Power: {CurrentPower} W Waiting for initial charge to reach, left: {SecondsLeft} sec",
                        currentPower,
                        (int)(_settings!.MinimumInitialChargeDuration.TotalSeconds -
                              timeSinceChargeStarted.TotalSeconds));
                }

                _log.LogDebug("Waiting for {Seconds} sec.", _settings!.PollTime.TotalSeconds);
                await Task.Delay(_settings.PollTime);
                await _stateMachine.FireAsync(eTrigger.Charging);
            }
        }

        private async Task OnInitialChargeDurationReachedAsync()
        {
            // we first get the current vehicle data again to get the current charge power from the vehicle
            // then we calculate the new charge power and set it
            try
            {
                // this will also update the vehicle charge power
                LatestVehicleData = await _tesla.GetVehicleDataAsync();
                _log.LogInformation("Vehicle data: {Data}", LatestVehicleData);

                await _hubService.SendVehicleDataAsync(LatestVehicleData);
            }
            catch (Exception ex)
            {
                _log.LogError("Failed to set charge amps, Error: '{Error}'", ex.Message);
                await _stateMachine.FireAsync(eTrigger.Error);
                return;
            }

            // remember when we are in monitoring mode
            _lastChargeMonitoringStart = DateTime.Now;

            // clear the history
            _powerHistory.Clear();
            _compensatedHistory.Clear();
            _totalPowerHistory.Clear();

            // now we can monitor the charge
            _log.LogDebug("Waiting for {Seconds} sec.", _settings!.PollTime.TotalSeconds);
            await Task.Delay(_settings.PollTime);
            await _stateMachine.FireAsync(eTrigger.MonitorCharge);
        }

        private async Task OnMonitoringCharge()
        {
            if (_stopRequested)
            {
                await _stateMachine.FireAsync(eTrigger.StopCharge);
            }
            else
            {
                var validPower = false;
                var currentPower = 0;
                var compensatedPower = 0;
                try
                {
                    var activePower = await _powerMeter.GetActivePowerAsync();
                    currentPower = activePower.Values.Sum();
                    compensatedPower = currentPower - _tesla.CalculateChargePower(_numPhases);

                    _compensatedHistory.Add(
                        new KeyValuePair<DateTime, int>(DateTime.Now, compensatedPower));

                    await AddPowerAsync(currentPower, compensatedPower);
                    validPower = true;
                }
                catch (Exception ex)
                {
                    _log.LogError("Failed to get active power, Error: '{Error}'", ex.Message);
                }

                if (validPower)
                {
                    // now we will wait until the minimum current duration is reached
                    // before we potentially change the current
                    var compensatedAverage =
                        GetCompensatedMovingAverage(_lastChargeMonitoringStart, _settings!.MinimumCurrentDuration);
                    if (compensatedAverage.TimeReached)
                    {
                        _log.LogInformation(
                            "Charging minimum duration reached Current power: {CurrentPower} W CompensatedPower: {CompensatedPower} , Moving average: {MovingAverage} Needs to be below: {Below} W",
                            currentPower, compensatedPower, compensatedAverage.Value, _settings!.EnoughSolarWatt);
                        await _stateMachine.FireAsync(eTrigger.MonitorChargeDurationReached);
                        return;
                    }

                    _log.LogInformation(
                        "Current power: {CurrentPower} W CompensatedPower: {CompensatedPower} , Moving average: {MovingAverage} Needs to be below: {Below} W, Time Left: {TimeLeft} sec.",
                        currentPower, compensatedPower, compensatedAverage.Value, _settings!.EnoughSolarWatt,
                        (int?)compensatedAverage.TimeLeft?.TotalSeconds);
                }

                _log.LogDebug("Waiting for {Seconds} sec.", _settings!.PollTime.TotalSeconds);
                await Task.Delay(_settings.PollTime);
                await _stateMachine.FireAsync(eTrigger.MonitorCharge);
            }
        }

        private async Task OnMonitorChargeDurationReachedAsync()
        {
            // we always get the vehicle data now to get the correct power
            // and check for the battery level
            try
            {
                LatestVehicleData = await _tesla.GetVehicleDataAsync();
                _log.LogInformation("Vehicle data: {VehicleData}", LatestVehicleData);
            }
            catch (Exception exp)
            {
                _log.LogError("Failed to get vehicle data, Error: '{Error}'", exp.Message);
                await _stateMachine.FireAsync(eTrigger.Error);
                return;
            }

            await _hubService.SendVehicleDataAsync(LatestVehicleData);

            // we check if we reached the battery level
            // if so, we can stop charging already
            if (_tesla.CurrentBatteryLevel >= LatestVehicleData.ChargeState?.ChargeLimitSoc)
            {
                _log.LogInformation("Battery level: {Level} reached, stopping charge", _tesla.CurrentBatteryLevel);
                await _stateMachine.FireAsync(eTrigger.StopCharge);
                return;
            }

            // based on the compensated value we will adjust the charge power
            var compensatedAverage =
                GetCompensatedMovingAverage(_lastChargeMonitoringStart, _settings!.MinimumCurrentDuration);

            // first check if we still have enough solar power
            if (compensatedAverage.Value! > _settings!.EnoughSolarWatt)
            {
                // not enough solar power
                // check if we have reached the minimum charge duration
                var timeSinceChargeStart = DateTime.Now - _lastChargeStart;
                if (timeSinceChargeStart > _settings.MinimumChargeDuration)
                {
                    _log.LogWarning(
                        "Stop charging, not enough solar power, needed: {Needed} W have: {Have} Time since started: {Seconds} sec.",
                        _settings!.EnoughSolarWatt, compensatedAverage.Value, timeSinceChargeStart.TotalSeconds);
                    await _stateMachine.FireAsync(eTrigger.StopCharge);
                    return;
                }

                _log.LogWarning(
                    "Not enough solar power, needed: {Needed} W have: {Have} but minimum charge duration not reached Time since started: {Seconds} sec.",
                    _settings!.EnoughSolarWatt, compensatedAverage.Value, timeSinceChargeStart.TotalSeconds);
            }

            var availablePower = compensatedAverage.Value! * -1;
            var possibleAmps = GetAvailableCurrent((int)availablePower);
            if (possibleAmps != _tesla.CurrentChargingAmps)
            {
                _log.LogInformation("Updating current from: {CurrentAmps} A to {NewAmps} A", _tesla.CurrentChargingAmps,
                    possibleAmps);

                try
                {
                    if (_currentChargeSession != null)
                    {
                        var currentChange = new ChargeCurrentChange
                        {
                            Id = Guid.NewGuid(),
                            ChargeSessionId = _currentChargeSession.Id,
                            ChargeSession = _currentChargeSession,
                            Timestamp = DateTime.Now,
                            Current = possibleAmps
                        };
                        await _chargeSessionService.AddCurrentChangeAsync(currentChange);
                    }
                }
                catch (Exception exp)
                {
                    _log.LogError("Failed to add new current change, Error: '{Error}'", exp.Message);
                }

                try
                {
                    await _tesla.SetChargeAmpsAsync(possibleAmps);
                }
                catch (Exception exp)
                {
                    _log.LogError("Failed to set charge amps, Error: '{Error}'", exp.Message);
                    await _stateMachine.FireAsync(eTrigger.Error);
                    return;
                }
            }
            else
            {
                _log.LogInformation("No change in current needed, staying at: {Amps} A", possibleAmps);
            }

            // we now clear everything
            // and start monitoring again
            _compensatedHistory.Clear();
            _lastChargeMonitoringStart = DateTime.Now;

            _log.LogDebug("Waiting for {Seconds} sec.", _settings!.PollTime.TotalSeconds);
            await Task.Delay(_settings.PollTime);
            await _stateMachine.FireAsync(eTrigger.MonitorCharge);
        }

        private async Task OnStopChargeAsync()
        {
            try
            {
                await _tesla.StopChargeAsync();
            }
            catch (Exception ex)
            {
                _log.LogError("Failed to stop charging, Error: '{Error}'", ex.Message);
            }

            try
            {
                if (_currentChargeSession != null)
                {
                    LatestVehicleData = await _tesla.GetVehicleDataAsync();
                    _currentChargeSession.End = DateTime.Now;
                    _currentChargeSession.BatteryLevelEnded = _tesla.CurrentBatteryLevel;
                    _currentChargeSession.EnergyAdded = LatestVehicleData.ChargeState?.ChargeEnergyAdded ?? 0;
                    await _chargeSessionService.UpdateChargeSessionAsync(_currentChargeSession);

                    await _hubService.SendVehicleDataAsync(LatestVehicleData);
                }
            }
            catch (Exception exp)
            {
                _log.LogError("Failed to update charge sessions, Error: '{Error}'", exp.Message);
            }

            await _stateMachine.FireAsync(eTrigger.StopChargeDone);
        }
    }
}
