﻿using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SolarCharger.EF;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SolarCharger.Services.Objects;

namespace SolarCharger.Services
{
    public class Tesla : ITesla
    {
        private readonly ILogger<Tesla> _log;
        private readonly HttpClient _httpClient;
        private readonly ChargeContext _chargeContext;
        private Settings? _settings;
        private Token? _currentToken;

        public Tesla(ILogger<Tesla> log, ChargeContext chargeContext, HttpClient httpClient)
        {
            _log = log;
            _chargeContext = chargeContext;
            CurrentChargingAmps = 0;
            _httpClient = httpClient;
        }

        public int CurrentChargingAmps { get; set; }
        public int CurrentChargePower { get; set; }
        public int CurrentChargeVoltage { get; set; }
        public int CurrentBatteryLevel { get; set; }

        public async Task StartAsync()
        {
            try
            {
                _settings = await _chargeContext.Settings.FirstOrDefaultAsync();
            }
            catch (Exception exp)
            {
                throw new Exception($"Failed to get settings, Error: '{exp.Message}'");
            }


            _log.LogInformation("Getting token");
            await RefreshTokenAsync();
        }

        public async Task<bool> IsOnlineAsync()
        {
            var vehicle = await GetVehicleInformationAsync();
            _log.LogInformation("Vehicle: {Vehicle}", vehicle);
            return vehicle.IsOnline();
        }

        private async Task<Vehicle> GetVehicleInformationAsync()
        {
            if (_currentToken == null)
            {
                throw new Exception("No token available");
            }

            var url = $"{_settings!.TeslaCommandsAddress}/api/1/vehicles/{_settings.Vin}";
            Tuple<string, ResponseWrapper<Vehicle>?> vehicle = await GetAndDeserializeAsync<ResponseWrapper<Vehicle>>(url, _currentToken.AccessToken);
            if (vehicle.Item2?.Response == null)
            {
                throw new Exception("Failed to get vehicle information");
            }

            return vehicle.Item2.Response;
        }

        // expensive call
        public async Task<Tuple<string, VehicleData>> GetVehicleDataAsync()
        {
            if (_currentToken == null)
            {
                throw new Exception("No token available");
            }

            var url = $"{_settings!.TeslaCommandsAddress}/api/1/vehicles/{_settings.Vin}/vehicle_data";
            Tuple<string, ResponseWrapper<VehicleData>?> vehicle = await GetAndDeserializeAsync<ResponseWrapper<VehicleData>>(url, _currentToken.AccessToken);
            if (vehicle.Item2?.Response == null)
            {
                throw new Exception("Failed to get vehicle data");
            }

            CurrentBatteryLevel = vehicle.Item2.Response.ChargeState!.BatteryLevel;
            CurrentChargePower = vehicle.Item2.Response.ChargeState!.ChargePowerWatt;
            CurrentChargeVoltage = CurrentChargePower > 0 ? vehicle.Item2.Response.ChargeState!.ChargerVoltage : 240;

            // we return the parsed response but also the raw response so we can do something with it in the future
            return new Tuple<string, VehicleData>(vehicle.Item1, vehicle.Item2.Response);
        }

        public async Task SetChargeAmpsAsync(int amps)
        {
            _log.LogInformation("Setting charge amps to: {Amps}", amps);
            if (_currentToken == null)
            {
                throw new Exception("No token available");
            }

            var url = $"{_settings!.TeslaCommandsAddress}/api/1/vehicles/{_settings.Vin}/command/set_charging_amps";

            dynamic payload = new
            {
                charging_amps = amps
            };

            ResponseWrapper<BasicResponse>? response = await PostAndDeserializeAsync<ResponseWrapper<BasicResponse>>(url, payload, _currentToken.AccessToken);
            if (response?.Response == null)
            {
                throw new Exception("Failed to set charge amps");
            }

            if (!response.Response.Result)
            {
                throw new Exception($"Failed to set charging amps to: {amps} Response: '{response.Response.Reason}");
            }
            CurrentChargingAmps = amps;
            _log.LogInformation("Successfully set charge amps to: {Amps}", amps);

        }

        public async Task StartChargeAsync()
        {
            _log.LogInformation("Starting charge");
            if (_currentToken == null)
            {
                throw new Exception("No token available");
            }

            var url = $"{_settings!.TeslaCommandsAddress}/api/1/vehicles/{_settings.Vin}/command/charge_start";

            dynamic payload = new
            {

            };

            ResponseWrapper<BasicResponse>? response = await PostAndDeserializeAsync<ResponseWrapper<BasicResponse>>(url, payload, _currentToken.AccessToken);
            if (response?.Response == null)
            {
                throw new Exception("Failed to start charging");
            }

            if (!response.Response.Result)
            {
                throw new Exception($"Failed to set start charging Response: '{response.Response.Reason}");
            }
            _log.LogInformation("Successfully starting to charge");
        }

        public async Task StopChargeAsync()
        {
            _log.LogInformation("Stopping charge");
            if (_currentToken == null)
            {
                throw new Exception("No token available");
            }

            var url = $"{_settings!.TeslaCommandsAddress}/api/1/vehicles/{_settings.Vin}/command/charge_stop";

            dynamic payload = new
            {

            };

            ResponseWrapper<BasicResponse>? response = await PostAndDeserializeAsync<ResponseWrapper<BasicResponse>>(url, payload, _currentToken.AccessToken);
            if (response?.Response == null)
            {
                throw new Exception("Failed to stop charging");
            }

            if (!response.Response.Result)
            {
                throw new Exception($"Failed to set stop charging Response: '{response.Response.Reason}");
            }
            _log.LogInformation("Successfully stopped charge");
        }

        public int CalculateChargePower(int phases)
        {
            return CurrentChargingAmps * phases * CurrentChargeVoltage;
        }

        private async Task<Token?> RefreshTokenAsync()
        {
            if (_settings != null)
            {
                var needsRefresh = true;
                var currentRefreshToken = _settings.TeslaRefreshToken;
                if (_currentToken != null)
                {
                    currentRefreshToken = _currentToken.RefreshToken;

                    if (_currentToken.IsAlmostExpired)
                    {
                        _log.LogInformation("RefreshToken is almost expired, needs refresh");
                        needsRefresh = true;
                    }
                }

                if (needsRefresh)
                {
                    _log.LogInformation("Refreshing token");
                    var url = $"{_settings.TeslaFleetAddress}/oauth2/v3/token";
                    dynamic payload = new
                    {
                        grant_type = "refresh_token",
                        client_id = _settings.TeslaClientId,
                        refresh_token = currentRefreshToken
                    };

                    Token? newToken = await PostAndDeserializeAsync<Token>(url, payload);
                    if (newToken != null)
                    {
                        _log.LogInformation("New Token: '{Token}'", newToken);
                        _currentToken = newToken;
                        return newToken;
                    }
                }
            }

            return null;
        }

        public async Task<T?> PostAndDeserializeAsync<T>(string url, object payload, string? token = null)
        {
            if (token != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            _log.LogInformation("Post for Url: '{Url}' has response: '{Response}'", url, jsonResponse);
            var result = JsonConvert.DeserializeObject<T>(jsonResponse);

            return result;
        }

        public async Task<Tuple<string, T?>> GetAndDeserializeAsync<T>(string url, string? token = null)
        {
            if (token != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            _log.LogInformation("Post for Url: '{Url}' has response: '{Response}'", url, jsonResponse);
            var result = JsonConvert.DeserializeObject<T>(jsonResponse);
            return new Tuple<string, T?>(jsonResponse, result);
        }
    }
}
