using System.ComponentModel.DataAnnotations;

namespace SolarCharger.EF
{
    public class ChargePower
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ChargeSessionId { get; set; }
        public ChargeSession ChargeSession { get; set; }
        [Required] public DateTime Timestamp { get; set; }
        [Required] public int Power { get; set; }
        public int? CompensatedPower { get; set; }
    }
}
