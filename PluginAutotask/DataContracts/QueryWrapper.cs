using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginAutotask.DataContracts
{
    public class QueryWrapper
    {
        [JsonProperty("items")]
        public List<Dictionary<string, object>> Items { get; set; }

        [JsonProperty("pageDetails")]
        public PageDetails PageDetails { get; set;}
    }

    public class PageDetails
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("requestCount")]
        public long RequestCount { get; set; }

        [JsonProperty("prevPageUrl")]
        public string? PrevPageUrl { get; set; }

        [JsonProperty("nextPageUrl")]
        public string? NextPageUrl { get; set; }
    }
}