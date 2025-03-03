namespace SolarCharger.Controllers.ViewModels
{
    public class PowerHistoryViewModel
    {
        public DateTime Time { get; set; }
        public int Power { get; set; }
        public int? CompensatedPower { get; set; }

        public static PowerHistoryViewModel FromModel(PowerHistory model)
        {
            return new PowerHistoryViewModel
            {
                Time = model.Time,
                Power = model.Power,
                CompensatedPower = model.CompensatedPower
            };
        }
    }
}
