using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginAutotask.DataContracts
{
    public class FieldsWrapper
    {
        [JsonProperty("fields")]
        public List<Field> Fields { get; set; }
    }

    public class Field
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("dataType")]
        public string DataType { get; set; }

        [JsonProperty("isRequired")]
        public bool IsRequired { get; set; }
    }
}