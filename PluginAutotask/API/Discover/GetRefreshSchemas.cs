using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using PluginAutotask.API.Factory;

namespace PluginAutotask.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetRefreshSchemas(IApiClient apiClient, RepeatedField<Schema> refreshSchemas, int sampleSize = 5)
        {
            var totalSchemas = refreshSchemas.Count;
            var processedCount = 0;
            var successCount = 0;
            var failureCount = 0;
            
            foreach (var refreshSchema in refreshSchemas)
            {
                processedCount++;
                var schema = refreshSchema;
                Logger.Debug($"Refreshing schema '{schema.Id}' ({processedCount}/{totalSchemas})");
                
                Schema? resultSchema = null;
                
                try
                {
                    if (string.IsNullOrWhiteSpace(schema.Query))
                    {
                        // not user defined
                        resultSchema = await AddPropertiesForEntity(apiClient, schema);
                        resultSchema = await AddSampleAndCount(apiClient, resultSchema, sampleSize);
                    }
                    else
                    {
                        // user defined
                        var userDefinedQuery = Utility.Utility.ParseUserDefinedQuery(schema.Query);

                        resultSchema = await AddPropertiesForEntity(apiClient, schema, userDefinedQuery);
                        resultSchema = await AddSampleAndCount(apiClient, resultSchema, sampleSize, userDefinedQuery);
                    }
                    
                    successCount++;
                    Logger.Debug($"Successfully refreshed schema '{schema.Id}' ({processedCount}/{totalSchemas})");
                }
                catch (Exception e)
                {
                    failureCount++;
                    Logger.Error(e, $"Failed to refresh schema '{schema.Id}' ({processedCount}/{totalSchemas}). Error: {e.Message}");
                    resultSchema = null;
                    // Continue with next schema instead of failing entire refresh
                }
                
                if (resultSchema != null)
                {
                    yield return resultSchema;
                }
                
                // Add delay to prevent API rate limiting
                await Task.Delay(200);
            }
            
            Logger.Debug($"Schema refresh completed. Total: {processedCount}, Success: {successCount}, Failed: {failureCount}");
        }
    }
}