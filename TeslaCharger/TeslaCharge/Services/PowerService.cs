namespace TeslaCharge.Services
{
    public class PowerService : IPowerService
    {
        public event Action<int>? OnPowerChanged;
        public int ActivePower { get; private set; }

        public void SetCurrentPower(int power)
        {
            ActivePower = power;
            OnPowerChanged?.Invoke(power);
        }
    }
}
