using System;
using System.Linq;
using Newtonsoft.Json;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Utility
{
    public static partial class Utility
    {
        public static UserDefinedQuery ParseUserDefinedQuery(string query)
        {
            var lines = query.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.None
            ).ToList();

            if (lines.Count != 2)
            {
                throw new Exception("Exactly 2 lines must be supplied. The first line must be the entity id and the second line must be a filter query.");
            }

            try
            {
                return new UserDefinedQuery()
                {
                    EntityId = lines[0],
                    Query = JsonConvert.DeserializeObject<Query>(lines[1]),
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing filter query: {e.Message}");
            }
        }
    }
}