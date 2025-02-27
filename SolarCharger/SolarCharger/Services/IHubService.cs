namespace SolarCharger.Services
{
    public interface IHubService
    {
        Task SendStateChangedAsync(eState state);
    }
}
