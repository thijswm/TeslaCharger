using Newtonsoft.Json;

namespace SolarCharger.Services.Objects
{
    public class ResponseWrapper<T>
    {
        [JsonProperty("response")] public T? Response { get; set; }
    }
}
