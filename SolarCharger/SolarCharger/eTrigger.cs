namespace SolarCharger
{
    public enum eTrigger
    {
        Start,
        Started,
        TeslaCheckOnline,
        TeslaOnline,
        TeslaCanCharge,
        EnoughSolarPower,
        StartCharge,
        StopCharge,
        StopChargeDone,
        Charging,
        InitialChargeDurationReached,
        MonitorCharge,
        MonitorChargeDurationReached,
        Error
    }
}
