using System.Collections.Generic;

namespace PluginHubspot.DataContracts
{
    public class DataWrapper
    {
        public long PageSize { get; set; }
        public long PageNumber { get; set; }
        public long TotalRecords { get; set; }
        public long TotalPages { get; set; }
        public List<Dictionary<string, object>>? Items { get; set; }
    }
}