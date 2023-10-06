using System.Collections.Generic;
using Google.Protobuf.Collections;
using Aunalytics.Sdk.Plugins;
using PluginAutotask.API.Factory;
using PluginAutotask.API.Utility;
using PluginAutotask.Helper;

namespace PluginAutotask.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetRefreshSchemas(IApiClient apiClient, RepeatedField<Schema> refreshSchemas, int sampleSize = 5)
        {
            foreach (var refreshSchema in refreshSchemas)
            {
                var schema = refreshSchema;
                // not user defined
                if (Constants.EntitiesList.Contains(schema.Id))
                {
                    schema = await AddPropertiesForEntity(apiClient, schema);
                    schema = await AddSampleAndCount(apiClient, schema, sampleSize);

                    yield return schema;
                }

                // TODO: add support for user defined schemas
                yield return schema;
            }
        }
    }
}