namespace TeslaCharge.Services
{
    public class TeslaService : ITeslaService
    {
        public event Action<TeslaData>? OnDataChanged;
        public TeslaData LatestData { get; private set; }

        public void UpdateData(TeslaData data)
        {
            LatestData = data;
            OnDataChanged?.Invoke(data);
        }
    }
}
