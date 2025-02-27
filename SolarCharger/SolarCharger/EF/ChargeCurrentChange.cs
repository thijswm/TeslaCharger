﻿using System.ComponentModel.DataAnnotations;

namespace SolarCharger.EF
{
    public class ChargeCurrentChange
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ChargeSessionId { get; set; }
        public ChargeSession ChargeSession { get; set; }

        [Required] public DateTime Timestamp { get; set; }
        [Required] public int Current { get; set; }
    }
}
