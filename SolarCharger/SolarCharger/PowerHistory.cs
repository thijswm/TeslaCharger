namespace SolarCharger
{
    public class PowerHistory
    {
        public DateTime Time { get; set; }
        public int Power { get; set; }
        public int? CompensatedPower { get; set; }
    }
}
