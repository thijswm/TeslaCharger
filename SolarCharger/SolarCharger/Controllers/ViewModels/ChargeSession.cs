using SolarCharger.EF;

namespace SolarCharger.Controllers.ViewModels
{
    public class ChargeSessionViewModel
    {
        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public int BatteryLevelStarted { get; set; }
        public int? BatteryLevelEnded { get; set; }
        public double EnergyAdded { get; set; }

        public static ChargeSessionViewModel FromChargeSession(ChargeSession chargeSession)
        {
            return new ChargeSessionViewModel
            {
                Id = chargeSession.Id,
                Start = chargeSession.Start,
                End = chargeSession.End,
                BatteryLevelStarted = chargeSession.BatteryLevelStarted,
                BatteryLevelEnded = chargeSession.BatteryLevelEnded,
                EnergyAdded = chargeSession.EnergyAdded
            };
        }
    }
}
