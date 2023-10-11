using System.Collections.Generic;
using Aunalytics.Sdk.Plugins;
using PluginAutotask.API.Factory;
using PluginAutotask.API.Utility;

namespace PluginAutotask.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetAllSchemas(IApiClient apiClient, int sampleSize = 5)
        {
            foreach (var entity in Constants.EntitiesList) 
            {
                // base schema to be added to
                var schema = new Schema
                {
                    Id = entity,
                    Name = entity,
                    Description = "",
                    DataFlowDirection = Schema.Types.DataFlowDirection.Read,
                };

                schema = await AddPropertiesForEntity(apiClient, schema);
                schema = await AddSampleAndCount(apiClient, schema, sampleSize);

                yield return schema;
            }
        }
    }
}