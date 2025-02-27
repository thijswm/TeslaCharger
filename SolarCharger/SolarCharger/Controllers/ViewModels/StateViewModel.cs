namespace SolarCharger.Controllers.ViewModels
{
    public class StateViewModel
    {
        public eState State { get; set; }

        public static StateViewModel FromState(eState state)
        {
            return new StateViewModel
            {
                State = state
            };
        }
    }
}
