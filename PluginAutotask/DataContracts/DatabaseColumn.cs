using System.Collections.Generic;

namespace PluginHubspot.DataContracts
{
    public class DatabaseColumnsWrapper
    {
        public List<DatabaseColumn>? DatabaseColumns { get; set; }
    }
    
    public class DatabaseColumn
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public long ColumnSize { get; set; }
        public string IsCustom { get; set; }
        public string Variable { get; set; }
    }
}