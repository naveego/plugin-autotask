using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginAutotask.DataContracts
{
    public class QueryCountWrapper
    {
        [JsonProperty("queryCount")]
        public int QueryCount { get; set; }
    }
}