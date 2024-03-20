using System;
using System.Collections.Generic;
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
        public static async Task<Schema> AddPropertiesForEntity(IApiClient apiClient, Schema schema, UserDefinedQuery? userDefinedQuery = null)
        {
            var unknownFieldCount = 1;
            var properties = new List<Property>();
            var entityId = schema.Id;

            if (userDefinedQuery != null)
            {
                entityId = userDefinedQuery.EntityId;
            }
            else if (Constants.IsRangedTicketHistoryName(schema.Id))
            {
                entityId = Constants.EntityTicketHistory;
            }

            try
            {
                var fieldsResult = await apiClient.GetAsync($"/{entityId}/entityInformation/fields");
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
                var fieldsResult = await apiClient.GetAsync($"/{entityId}/entityInformation/userDefinedFields");
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
    }
}