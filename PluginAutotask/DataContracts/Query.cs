using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginAutotask.DataContracts
{
    public class Query
    {
        [JsonProperty("MaxRecords", NullValueHandling=NullValueHandling.Ignore)]
        public int MaxRecords { get; set; }

        [JsonProperty("Filter")]
        public List<Filter> Filter { get; set; }
    }

    public class Filter
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("op")]
        public string Operation { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }
    }
}