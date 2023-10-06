using System.Collections.Generic;
using Google.Protobuf.Collections;
using Aunalytics.Sdk.Plugins;
using PluginHubspot.API.Factory;
using PluginHubspot.API.Utility;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetRefreshSchemas(IApiClient apiClient, Settings settings,
            RepeatedField<Schema> refreshSchemas, int sampleSize = 5)
        {
            foreach (var schema in refreshSchemas)
            {
                var endpoint = EndpointHelper.GetEndpointForSchema(schema);

                var refreshSchema = await GetSchemaForEndpoint(apiClient, schema, endpoint);

                // get sample and count
                yield return await AddSampleAndCount(apiClient,  refreshSchema, settings, sampleSize, endpoint);
            }
        }
    }
}