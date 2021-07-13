using Newtonsoft.Json;

namespace PluginHubspot.DataContracts
{
    public class ApiError
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}