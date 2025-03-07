using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SolarChargerFE.Components.Pages
{
    public partial class Reports : MudComponentBase
    {
        [Inject] public SolarChargerClient _client { get; set; }
        [Inject] private ISnackbar _snackBar { get; set; }
        private bool _loading = false;
        private string? _loadError;
        private List<ChargeSessionViewModel> _chargeSessions = new();

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            try
            {
                _chargeSessions = new List<ChargeSessionViewModel>(await _client.Get_charge_sessionsAsync());
            }
            catch (Exception exp)
            {
                _loadError = $"Failed to load existing settings, Error: '{exp.Message}'";
            }
            finally
            {
                _loading = false;
            }
        }
    }
}
