using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SolarCharger.Controllers.ViewModels;
using SolarCharger.Services;

namespace SolarCharger.Controllers
{
    public class SolarController : ControllerBase
    {
        private readonly IStateEngine _stateEngine;
        private readonly SettingsService _settingsService;
        private readonly ChargeSessionService _chargeSessionService;

        public SolarController(IStateEngine stateEngine, SettingsService settingsService, ChargeSessionService chargeSessionService)
        {
            _stateEngine = stateEngine;
            _settingsService = settingsService;
            _chargeSessionService = chargeSessionService;
        }


        [HttpGet]
        [Route("api/get_state")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StateViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetState()
        {
            try
            {
                return Ok(StateViewModel.FromState(_stateEngine.State));
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to get state, Error: '{ex.Message}'");
            }
        }


        [HttpPost]
        [Route("api/start_charge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult StartCharge()
        {
            try
            {
                _ = _stateEngine.FireStartAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to start charge, Error: '{ex.Message}'");
            }
        }

        [HttpPost]
        [Route("api/stop_charge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult StopCharge()
        {
            try
            {
                _ = _stateEngine.FireStopAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to stop charge, Error: '{ex.Message}'");
            }
        }

        [HttpPost]
        [Route("api/update_settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> UpdateSettings([FromBody] SettingsViewModel settings)
        {
            try
            {
                await _settingsService.UpdateSettingsAsync(settings.ToModel());
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to update settings, Error: '{ex.Message}'");
            }
        }

        [HttpGet]
        [Route("api/get_settings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SettingsViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                var settings = await _settingsService.GetSettingsAsync();
                if (settings == null)
                {
                    return NotFound();
                }
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to get settings, Error: '{ex.Message}'");
            }
        }

        [HttpGet]
        [Route("api/get_charge_sessions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChargeSessionViewModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetChargeSessions()
        {
            try
            {
                var chargeSessions = await _chargeSessionService.GetChargeSessionsAsync();
                return Ok(chargeSessions);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to get charge sessions, Error: '{ex.Message}'");
            }
        }

        [HttpGet]
        [Route("api/get_charge_current_changes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChargeCurrentChangeViewModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetChargeCurrentChanges()
        {
            try
            {
                var chargeCurrentChanges = await _chargeSessionService.GetCurrentChangesAsync();
                return Ok(chargeCurrentChanges);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to get charge current changes, Error: '{ex.Message}'");
            }
        }
    }
}
