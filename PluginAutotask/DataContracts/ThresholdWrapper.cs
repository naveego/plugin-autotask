using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginAutotask.DataContracts
{
    public class ThresholdWrapper
    {
        [JsonProperty("externalRequestThreshold")]
        public int ExternalRequestThreshold { get; set; }

        [JsonProperty("requestThresholdTimeframe")]
        public int RequestThresholdTimeframe { get; set; }

        [JsonProperty("currentTimeframeRequestCount")]
        public int CurrentTimeframeRequestCount { get; set; }
    }
}