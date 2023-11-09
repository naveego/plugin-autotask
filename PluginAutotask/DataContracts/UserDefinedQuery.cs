using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginAutotask.DataContracts
{
    public class UserDefinedQuery
    {
        [JsonProperty("query")]
        public Query Query { get; set; }

        [JsonProperty("entityId")]
        public string EntityId { get; set; }
    }
}