using System;
using System.Threading.Tasks;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginAutotask.API.Factory;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Discover
{
    public static partial class Discover
    {
        public static async Task<Count> GetCountOfRecords(IApiClient apiClient, Schema schema, UserDefinedQuery? userDefinedQuery = null)
        {
            var entityId = schema.Id;
            var query = Utility.Utility.GetDefaultQueryForEntityId(schema.Id);

            if (userDefinedQuery != null) 
            {
                entityId = userDefinedQuery.EntityId;
                query = userDefinedQuery.Query;
            }
            
            var countResult = await apiClient.GetAsync($"/{entityId}/query/count?search={JsonConvert.SerializeObject(query)}");
            
            try
            {
                countResult.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }

            var countWrapper = JsonConvert.DeserializeObject<QueryCountWrapper>(await countResult.Content.ReadAsStringAsync());

            return new Count() 
            {
                Kind = Count.Types.Kind.Exact,
                Value = countWrapper.QueryCount,
            };
        }
    }
}