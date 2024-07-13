using Microsoft.AspNetCore.Components;
using MudBlazor;
using TeslaCharge.Services;

namespace TeslaCharge.Components.Pages
{
    public partial class Home : MudComponentBase
    {
        [Inject] public IPowerService PowerService { get; set; }
        [Inject] public ITeslaService TeslaService { get; set; }
        [Inject] public ITeslaCharger TeslaCharger { get; set; }

        protected override void OnInitialized()
        {
            PowerService.OnPowerChanged += PowerService_OnPowerChanged;
            TeslaService.OnDataChanged += TeslaService_OnDataChanged;
            TeslaCharger.OnStateChanged += TeslaCharger_OnStateChanged;
        }

        private void TeslaCharger_OnStateChanged(State obj)
        {
            _ = InvokeAsync(StateHasChanged);
        }

        private void TeslaService_OnDataChanged(TeslaData obj)
        {
            _ = InvokeAsync(StateHasChanged);
        }

        private void PowerService_OnPowerChanged(int powerWatt)
        {
            _ = InvokeAsync(StateHasChanged);
        }

        private int? ActivePower => PowerService.ActivePower;
        private TeslaData TeslaData => TeslaService.LatestData;
        private State CurrentState => TeslaCharger.CurrentState;
    }
}
