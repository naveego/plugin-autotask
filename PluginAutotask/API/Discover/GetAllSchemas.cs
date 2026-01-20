using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using PluginAutotask.API.Factory;
using PluginAutotask.API.Utility;

namespace PluginAutotask.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetAllSchemas(IApiClient apiClient, int sampleSize = 5)
        {
            var totalEntities = Constants.EntitiesList.Count;
            var processedCount = 0;
            var successCount = 0;
            var failureCount = 0;

            foreach (var entity in Constants.EntitiesList)
            {
                processedCount++;
                Logger.Debug($"Discovering schema for entity '{entity}' ({processedCount}/{totalEntities})");
                
                Schema? schema = null;
                
                try
                {
                    // base schema to be added to
                    schema = new Schema
                    {
                        Id = entity,
                        Name = entity,
                        Description = "",
                        DataFlowDirection = Schema.Types.DataFlowDirection.Read,
                    };

                    schema = await AddPropertiesForEntity(apiClient, schema);
                    schema = await AddSampleAndCount(apiClient, schema, sampleSize);

                    successCount++;
                    Logger.Debug($"Successfully discovered schema for entity '{entity}'");
                }
                catch (Exception e)
                {
                    failureCount++;
                    Logger.Error(e, $"Failed to discover schema for entity '{entity}' ({processedCount}/{totalEntities}). Error: {e.Message}");
                    schema = null;
                    // Continue with next entity instead of failing entire discovery
                }
                
                if (schema != null)
                {
                    yield return schema;
                }
                
                // Add delay to prevent API rate limiting
                await Task.Delay(200);
            }
            
            Logger.Debug($"Schema discovery completed. Total: {processedCount}, Success: {successCount}, Failed: {failureCount}");
        }
    }
}