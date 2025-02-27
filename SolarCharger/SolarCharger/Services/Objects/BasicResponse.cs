using Newtonsoft.Json;

namespace SolarCharger.Services.Objects
{
    public class BasicResponse
    {
        [JsonProperty("result")]public bool Result { get; set; }
        [JsonProperty("reason")] public string? Reason { get; set; }
    }
}
