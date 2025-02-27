namespace SolarCharger.Services
{
    public interface IPowerMeter
    {
        Task<Dictionary<int, int>> GetActivePowerAsync();
    }
}
