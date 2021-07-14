using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginHubspot.API.Factory;
using PluginHubspot.API.Utility.EndpointHelperEndpoints;
using PluginHubspot.DataContracts;

namespace PluginHubspot.API.Utility
{
    public static class EndpointHelper
    {
        
        
        private static readonly Dictionary<string, Endpoint> Endpoints = new Dictionary<string, Endpoint>();

        static EndpointHelper()
        {
            ContactsEndpointHelper.ContactsEndpoints.ToList().ForEach(x => Endpoints.TryAdd(x.Key, x.Value));
            CompaniesEndpointHelper.CompaniesEndpoints.ToList().ForEach(x => Endpoints.TryAdd(x.Key, x.Value));
            TicketsEndpointHelper.TicketsEndpoints.ToList().ForEach(x => Endpoints.TryAdd(x.Key, x.Value));
            TasksEndpointHelper.TasksEndpoints.ToList().ForEach(x => Endpoints.TryAdd(x.Key, x.Value));
            ProjectsEndpointHelper.ProjectsEndpoints.ToList().ForEach(x => Endpoints.TryAdd(x.Key, x.Value));
            AttachmentInfoEndpointHelper.AttachmentInfoEndpoints.ToList().ForEach(x => Endpoints.TryAdd(x.Key, x.Value));
            
        }

        public static Dictionary<string, Endpoint> GetAllEndpoints()
        {
            return Endpoints;
        }

        public static Endpoint? GetEndpointForId(string id)
        {
            return Endpoints.ContainsKey(id) ? Endpoints[id] : null;
        }

        public static Endpoint? GetEndpointForSchema(Schema schema)
        {
            var endpointMetaJson = JsonConvert.DeserializeObject<dynamic>(schema.PublisherMetaJson);
            string endpointId = endpointMetaJson.Id;
            return GetEndpointForId(endpointId);
        }
    }

    public abstract class Endpoint
    {
        private class EndpointPropertyMetadataWrapper
        {
            [JsonProperty("fields")] public List<FieldPropertyMetadata> Fields { get; set; }
        }
        private class FieldPropertyMetadata
        {
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("isReference")] public string IsKey { get; set; }
            [JsonProperty("isRequired")] public string IsRequired { get; set; }
            [JsonProperty("dataType")] public string Type { get; set; }
        }
        private class UDFPropertyWrapper
        {
            [JsonProperty("items")]
            public List<UDFList> Items { get; set; }
        }

        private class UDFList
        {
            [JsonProperty("userDefinedFields")]
            public List<UDFListItem> UserDefinedFields { get; set; }
        }
        private class UDFListByRoot
        {
            [JsonProperty("root")]
            public List<UDFListItem> UserDefinedFields { get; set; }
        }

        private class UDFListItem
        {
            [JsonProperty("Name")]
            public string Name { get; set; }
            
            [JsonProperty("Value")]
            public string Value { get; set; }
        }
        
        private class UDFListItemRootless
        {
            public string Name { get; set; }
            
            public string Value { get; set; }
        }

        private class QueryResponseWrapper
        {
            [JsonProperty("items")] public List<Dictionary<string, object>> Items { get; set; }
            [JsonProperty("pageDetails")] public Dictionary<string, object> PageDetails { get; set; }
        }
        
        // private class PageDetails
        // {
        //     [JsonProperty("nextPageUrl")] public string NextPageUrl { get; set; }
        //     [JsonProperty("count")] public string Count { get; }
        // }
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string BasePath { get; set; } = "";
        public string AllPath { get; set; } = "";
        public List<string> PropertyKeys { get; set; } = new List<string>();

        public virtual bool ShouldGetStaticSchema { get; set; } = false;

        
        public List<EndpointActions> SupportedActions { get; set; } = new List<EndpointActions>();

        public virtual Task<Count> GetCountOfRecords(IApiClient apiClient)
        {
            return Task.FromResult(new Count
            {
                Kind = Count.Types.Kind.Unavailable,
            });
        }

        public virtual async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
            DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
        {
            string path = $"{AllPath.TrimStart('/')}";
            string nextPageUrl = "";
            
            do
            {
                var response = new HttpResponseMessage();
                if (!string.IsNullOrWhiteSpace(nextPageUrl) && path != nextPageUrl)
                {
                    path = nextPageUrl;
                    response = await apiClient.GetAsync(path, true);
                }
                else
                {
                    response = await apiClient.GetAsync(path);
                }
                
                response.EnsureSuccessStatusCode();

                var content =
                    JsonConvert.DeserializeObject<QueryResponseWrapper>(await response.Content.ReadAsStringAsync());

                if (content.Items.Count == 0)
                {
                    yield break;
                }

                foreach (var item in content.Items)
                {
                    var recordMap = new Dictionary<string, object>();

                    foreach (var field in item)
                    {
                        
                        if (field.Key != "userDefinedFields")
                        {
                            recordMap[field.Key] = field.Value?.ToString() ?? "null";
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(field.Value.ToString()))
                            {
                                var udfFields = JsonConvert.DeserializeObject<List<UDFListItemRootless>>(field.Value.ToString());

                                foreach (var udfField in udfFields)
                                {
                                    recordMap[udfField.Name] = udfField.Value?.ToString() ?? "null";
                                }
                            }
                            //deserialize field.value
                        }
                    }
                    yield return new Record
                    {
                        Action = Record.Types.Action.Upsert,
                        DataJson = JsonConvert.SerializeObject(recordMap)
                    };
                }

                if (content.PageDetails["nextPageUrl"] != null)
                {
                    nextPageUrl = content.PageDetails["nextPageUrl"].ToString();
                }
                else
                {
                    nextPageUrl = "";
                }
            } while (!string.IsNullOrWhiteSpace(nextPageUrl));
        }

        public virtual async Task<string> WriteRecordAsync(IApiClient apiClient, Schema schema, Record record,
            IServerStreamWriter<RecordAck> responseStream)
        {
            List<string> UDFFields = new List<string>();

            foreach (var field in schema.Properties)
            {
                if (field.TypeAtSource == "UserDefinedField-String")
                {
                    UDFFields.Add(field.Name);
                }
            }

            var dataJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(record.DataJson);

            Dictionary<string, string> payload = new Dictionary<string, string>();
            Dictionary<string, string> payloadUDF = new Dictionary<string, string>();

            foreach (var data in dataJson)
            {
                
                
                if (!UDFFields.Contains(data.Key))
                {
                    payload.Add(data.Key, data.Value);
                }
                else
                {
                    payloadUDF.Add(data.Key, data.Value);
                }
            }

            //Form user defined element json
            if (payloadUDF.Count > 0)
            {
                List<UDFListItem> userDefinedFieldList = new List<UDFListItem>();
                
                foreach (var field in payloadUDF)
                {
                    userDefinedFieldList.Add(
                        new UDFListItem()
                        {
                            Name = field.Key,
                            Value = field.Value
                        }
                    );
                }

                var userDefinedFieldElements = JsonConvert.SerializeObject(userDefinedFieldList);
                
                // string userDefinedFieldElement = "";
                //
                // userDefinedFieldElement += "[";
                // foreach (var field in payloadUDF)
                // {
                //     userDefinedFieldElement += "{";
                //
                //     userDefinedFieldElement += $"\"Name\": \"{field.Key}\",";
                //     userDefinedFieldElement += $"\"Value\": \"{field.Value}\"";
                //
                //     userDefinedFieldElement += "},";
                // }
                //
                // userDefinedFieldElement = userDefinedFieldElement.TrimEnd(',');
                // userDefinedFieldElement += "]";

                payload.Add("userDefinedFields", userDefinedFieldElements);
            }

            var payloadString = JsonConvert.SerializeObject(payload);

            //Remove some serialize side-effects
            // payloadString = payloadString.Replace("\"[", "[");
            // payloadString = payloadString.Replace("]\"", "]");
            // payloadString = payloadString.Replace("\\", "");

            var response = await apiClient.PatchAsync(BasePath, payloadString);

            return response.StatusCode.ToString();
        }

        public virtual async Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
        {
            var endpointId = JsonConvert.DeserializeObject<Dictionary<string, object>>(schema.PublisherMetaJson)["Id"];
            
            var propertyPath = $"atservicesrest/v1.0/{endpointId}/entityinformation/fields";
            
            var response = await apiClient.GetAsync(propertyPath);

            var endpointPropertyMetadataWrapper =
                JsonConvert.DeserializeObject<EndpointPropertyMetadataWrapper>(
                    await response.Content.ReadAsStringAsync());

            var properties = new List<Property>();

            foreach (var property in endpointPropertyMetadataWrapper.Fields)
            {
                properties.Add(new Property
                {
                    Id = property.Name,
                    Name = property.Name,
                    Description = "",
                    Type = Discover.Discover.GetPropertyType(property.Type),
                    TypeAtSource = property.Type,
                    //IsKey = Boolean.Parse(companyProperty.IsKey),
                    IsKey = property.Name == "id",
                    IsNullable = !Boolean.Parse(property.IsRequired),
                    IsCreateCounter = false,
                    IsUpdateCounter = false,
                });
            }
            
            var UDFPath = $"atservicesrest/v1.0/{endpointId}/query?search={{\"MaxRecords\":1, \"filter\":[{{\"op\":\"exist\",\"field\":\"id\"}}]}}";
            
            var udfResponse = await apiClient.GetAsync(UDFPath);

            udfResponse.EnsureSuccessStatusCode();
            
            var udfPropertyWrapper = JsonConvert.DeserializeObject<UDFPropertyWrapper>(await udfResponse.Content.ReadAsStringAsync());

            foreach (var udfField in udfPropertyWrapper.Items[0].UserDefinedFields)
            {
                try
                {
                    properties.Add(new Property
                    {
                        Id = udfField.Name,
                        Name = udfField.Name,
                        Description = "",
                        Type = PropertyType.String,
                        TypeAtSource = "UserDefinedField-String",
                        IsKey = false,
                        IsNullable = true,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                    });
                }
                catch (Exception e)
                {
                    var debug = e.Message;
                }
            }
            
            schema.Properties.Clear();
            schema.Properties.AddRange(properties);

            return schema;
        }

        public virtual Task<bool> IsCustomProperty(IApiClient apiClient, string propertyId)
        {
            return Task.FromResult(false);
        }

        public Schema.Types.DataFlowDirection GetDataFlowDirection()
        {
            if (CanRead() && CanWrite())
            {
                return Schema.Types.DataFlowDirection.ReadWrite;
            }

            if (CanRead() && !CanWrite())
            {
                return Schema.Types.DataFlowDirection.Read;
            }

            if (!CanRead() && CanWrite())
            {
                return Schema.Types.DataFlowDirection.Write;
            }

            return Schema.Types.DataFlowDirection.Read;
        }


        private bool CanRead()
        {
            return SupportedActions.Contains(EndpointActions.Get);
        }

        private bool CanWrite()
        {
            return SupportedActions.Contains(EndpointActions.Post) ||
                   SupportedActions.Contains(EndpointActions.Put) ||
                   SupportedActions.Contains(EndpointActions.Patch) ||
                   SupportedActions.Contains(EndpointActions.Delete);
        }
    }

    public enum EndpointActions
    {
        Get,
        Post,
        Put,
        Patch,
        Delete
    }
}