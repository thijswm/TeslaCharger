namespace SolarCharger
{
    public interface IStateEngine
    {
        eState State { get; }
        Task FireStartAsync();
        Task FireStopAsync();
    }
}
