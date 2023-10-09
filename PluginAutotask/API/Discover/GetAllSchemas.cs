using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginAutotask.API.Factory;
using PluginAutotask.API.Utility;
using PluginAutotask.DataContracts;
using PluginAutotask.Helper;

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

        private static async Task<Schema> AddPropertiesForEntity(IApiClient apiClient, Schema schema)
        {
            var unknownFieldCount = 1;
            var properties = new List<Property>();

            try
            {
                var fieldsResult = await apiClient.GetAsync($"/{schema.Id}/entityInformation/fields");
                fieldsResult.EnsureSuccessStatusCode();

                var fieldsWrapper = JsonConvert.DeserializeObject<FieldsWrapper>(await fieldsResult.Content.ReadAsStringAsync());
                foreach (var field in fieldsWrapper.Fields)
                {
                    var property = new Property()
                    {
                        Id = field.Name,
                        Name = field.Name,
                        Description = "",
                        Type = GetPropertyType(field.DataType),
                        TypeAtSource = field.DataType, 
                        IsKey = field.Name.ToLower().Equals("id"),
                        IsNullable = !field.IsRequired,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        PublisherMetaJson = "",
                    };

                    properties.Add(property);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }

            try
            {
                var fieldsResult = await apiClient.GetAsync($"/{schema.Id}/entityInformation/userDefinedFields");
                fieldsResult.EnsureSuccessStatusCode();

                var fieldsWrapper = JsonConvert.DeserializeObject<FieldsWrapper>(await fieldsResult.Content.ReadAsStringAsync());
                foreach (var field in fieldsWrapper.Fields)
                {
                    var property = new Property()
                    {
                        Id = field?.Name ?? $"UNKNOWN_{unknownFieldCount}",
                        Name = field?.Name ?? $"UNKNOWN_{unknownFieldCount}",
                        Description = Constants.UserDefinedProperty,
                        Type = GetPropertyType(field?.DataType),
                        TypeAtSource = field?.DataType ?? "", 
                        IsKey = field?.Name?.ToLower()?.Equals("id") ?? false,
                        IsNullable = !field?.IsRequired ?? true,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        PublisherMetaJson = "",
                    };

                    properties.Add(property);

                    if (field == null || field.Name == null) 
                    {
                        unknownFieldCount++;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }

            schema.Properties.Clear();
            schema.Properties.AddRange(properties);

            return schema;
        }

        private static async Task<Schema> AddSampleAndCount(IApiClient apiClient, Schema schema, int sampleSize)
        {
            // add sample and count
            var records = Read.Read.ReadRecordsAsync(apiClient, schema, sampleSize).Take(sampleSize);
            schema.Sample.AddRange(await records.ToListAsync());
            schema.Count = await GetCountOfRecords(apiClient, schema);

            return schema;
        }
    }
}