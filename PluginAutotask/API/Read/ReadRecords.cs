using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aunalytics.Sdk.Plugins;
using PluginHubspot.API.Factory;
using PluginHubspot.API.Utility;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Read
{
    public static partial class Read
    {
        public static async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient, Schema schema, Settings settings, DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null)
        {
            var endpoint = EndpointHelper.GetEndpointForSchema(schema);

            var records = endpoint?.ReadRecordsAsync(apiClient, lastReadTime, tcs);

            if (records != null)
            {
                await foreach (var record in records)
                {
                    yield return record;
                }
            }
        }
    }
}