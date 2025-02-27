using Newtonsoft.Json;

namespace SolarCharger.Services.Objects
{
    public class Token
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonProperty("refresh_token")] public string RefreshToken { get; set; } = string.Empty;
        [JsonProperty("expires_in")] public int ExpiresIn { get; set; }

        public DateTime ExpiresDateTime => DateTime.Now.AddSeconds(ExpiresIn);

        public bool IsAlmostExpired => ExpiresDateTime < DateTime.Now.AddMinutes(5);

        public override string ToString()
        {
            return $"AccessToken: '{AccessToken}' RefreshToken: '{RefreshToken}' ExpiresIn: {ExpiresDateTime} IsAlmostExpired: {IsAlmostExpired}";
        }
    }
}
