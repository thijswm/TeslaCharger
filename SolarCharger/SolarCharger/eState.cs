namespace SolarCharger
{
    public enum eState
    {
        Idle,
        Starting,
        IsTeslaOnline,
        TeslaOnline,
        CheckSolarPower,
        EnoughSolarPower,
        StartCharge,
        InitialCharging,
        InitialChargeDurationReached,
        MonitoringCharge,
        MonitoringChargeDurationReached,
        StopCharge,
    }
}
