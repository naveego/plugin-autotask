using System;
using System.Linq;
using System.Threading.Tasks;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginAutotask.API.Factory;
using PluginAutotask.API.Utility;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Discover
{
    public static partial class Discover
    {
        public static async Task<Count> GetCountOfRecords(IApiClient apiClient, Schema schema, UserDefinedQuery? userDefinedQuery = null)
        {
            var entityId = schema.Id;
            var query = Utility.Utility.GetDefaultQueryForEntityId(schema.Id);

            if (Constants.IsRangedTicketHistoryName(schema.Id))
            {
                var totalRecords = 0;
                await foreach (var count in Read.Read.CountRecordsForRangedTicketHistory(apiClient, schema))
                {
                    totalRecords += count;
                }

                return new Count
                {
                    Kind = Count.Types.Kind.Exact,
                    Value = totalRecords
                };
            }

            if (userDefinedQuery != null)
            {
                entityId = userDefinedQuery.EntityId;
                query = Utility.Utility.ApplyDynamicDate(userDefinedQuery.Query);
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