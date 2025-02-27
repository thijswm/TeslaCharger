using SolarCharger.EF;

namespace SolarCharger.Controllers.ViewModels
{
    public class ChargeCurrentChangeViewModel
    {
        public Guid Id { get; set; }
        public Guid ChargeSessionId { get; set; }
        public DateTime Timestamp { get; set; }
        public int Current { get; set; }

        public static ChargeCurrentChangeViewModel FromChargeCurrentChange(ChargeCurrentChange model)
        {
            return new ChargeCurrentChangeViewModel
            {
                Id = model.Id,
                ChargeSessionId = model.ChargeSessionId,
                Timestamp = model.Timestamp,
                Current = model.Current
            };
        }
    }
}
