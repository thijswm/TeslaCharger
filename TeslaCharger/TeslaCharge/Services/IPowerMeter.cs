namespace TeslaCharge.Services
{
    public interface IPowerMeter
    {
        Task<int> GetActivePowerAsync(string address);
    }
}
