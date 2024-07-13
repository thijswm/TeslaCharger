namespace TeslaCharge.Services
{
    public interface IPowerService
    {
        event Action<int>? OnPowerChanged;
        int ActivePower { get; }
    }
}
