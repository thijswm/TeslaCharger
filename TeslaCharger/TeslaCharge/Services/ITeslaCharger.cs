namespace TeslaCharge.Services
{
    public enum State
    {
        NotPluggedIn,
        PluggedIn,
        NotEnoughSolar,
        StartCharging
    }

    public interface ITeslaCharger
    {
        event Action<State>? OnStateChanged;
        State CurrentState { get; }
    }
}
