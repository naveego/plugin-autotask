using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PluginAutotask.DataContracts
{
    public class Query : ICloneable
    {
        [JsonProperty("MaxRecords", NullValueHandling=NullValueHandling.Ignore)]
        public int MaxRecords { get; set; }

        [JsonProperty("Filter")]
        public List<Filter> Filter { get; set; }

        [JsonProperty("IncludeFields", NullValueHandling=NullValueHandling.Ignore)]
        public List<string>? IncludeFields { get; set; }

        public Query Clone()
        {
            var clone = new Query
            {
                MaxRecords = this.MaxRecords,
                Filter = new List<Filter>(),
                IncludeFields = this.IncludeFields == null ? null : new List<string>()
            };

            clone.Filter.AddRange(this.Filter.Select(f => f.Clone()));

            if (this.IncludeFields != null)
                clone.IncludeFields!.AddRange(this.IncludeFields);

            return clone;
        }

        object ICloneable.Clone() => Clone();
    }

    public class Filter: ICloneable
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("op")]
        public string Operation { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        public Filter Clone() => new Filter
        {
            Field = this.Field,
            Operation = this.Operation,
            Value = this.Value
        };

        object ICloneable.Clone() => Clone();
    }
}