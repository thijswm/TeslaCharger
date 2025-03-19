namespace SolarChargerFE.Components.Pages.ViewModel
{
    public class ChargeSession
    {
        private readonly ChargeSessionViewModel _model;

        public ChargeSession(ChargeSessionViewModel model)
        {
            _model = model;
        }

        public Guid Id => _model.Id;
        public DateTime StartDate => _model.Start.LocalDateTime;
        public DateTime? EndDate => _model.End?.LocalDateTime;

        public int? BatteryLevelIncreased
        {
            get
            {
                if (_model.BatteryLevelEnded.HasValue)
                {
                    return _model.BatteryLevelEnded.Value - _model.BatteryLevelStarted;
                }
                return null;
            }
        }
    }
}
