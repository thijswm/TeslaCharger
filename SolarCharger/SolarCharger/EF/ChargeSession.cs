using System.ComponentModel.DataAnnotations;

namespace SolarCharger.EF
{
    public class ChargeSession
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime Start { get; set; }

        public DateTime? End { get; set; }

        [Required]
        public int BatteryLevelStarted { get; set; }
        public int? BatteryLevelEnded { get; set; }
        public double EnergyAdded { get; set; }
    }
}
