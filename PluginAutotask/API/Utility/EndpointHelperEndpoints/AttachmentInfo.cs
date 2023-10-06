using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginHubspot.API.Factory;
using PluginHubspot.DataContracts;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public class AttachmentInfoEndpointHelper
    {

        private class AttachmentInfoResponseWrapper
        {
            [JsonProperty("items")] public List<Dictionary<string, object>> AttachmentInfo { get; set; }
        }
        private class AttachmentInfoResponse
        {
            [JsonProperty("attachmentInfo")] public List<AttachmentInfo> AttachmentInfo { get; set; }
        }

        private class AttachmentInfo
        {
            [JsonProperty("properties")] public Dictionary<string, AttachmentInfoProperty> Properties { get; set; }
        }

        private class AttachmentInfoProperty
        {
            [JsonProperty("value")] public object Value { get; set; }
        }
        private class AttachmentInfoPropertyMetadataWrapper
        {
            [JsonProperty("fields")] public List<AttachmentInfoPropertyMetadata> Fields { get; set; }
        }
        private class AttachmentInfoPropertyMetadata
        {
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("isReference")] public string IsKey { get; set; }
            [JsonProperty("isRequired")] public string IsRequired { get; set; }
            [JsonProperty("dataType")] public string Type { get; set; }
        }

        private class AttachmentInfoEndpoint : Endpoint
        {
            private const string AttachmentInfoPropertiesPath = "atservicesrest/v1.0/AttachmentInfo/entityinformation/fields";

            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override async Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                // invoke attachmentInfo properties api
                var response = await apiClient.GetAsync(AttachmentInfoPropertiesPath);

                var companyPropertyWrapper =
                    JsonConvert.DeserializeObject<AttachmentInfoPropertyMetadataWrapper>(
                        await response.Content.ReadAsStringAsync());

                var properties = new List<Property>();

                foreach (var companyProperty in companyPropertyWrapper.Fields)
                {
                    properties.Add(new Property
                    {
                        Id = companyProperty.Name,
                        Name = companyProperty.Name,
                        Description = "",
                        Type = Discover.Discover.GetPropertyType(companyProperty.Type),
                        TypeAtSource = companyProperty.Type,
                        IsKey = Boolean.Parse(companyProperty.IsKey),
                        IsNullable = !Boolean.Parse(companyProperty.IsRequired),
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                    });
                }
                
                schema.Properties.AddRange(properties);

                return schema;
            }

            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                string path = $"{AllPath.TrimStart('/')}";
                string nextPageUrl = "";
                
                do
                {
                    var response = new HttpResponseMessage();
                    if (!string.IsNullOrWhiteSpace(nextPageUrl))
                    {
                        path = nextPageUrl;
                        response = await apiClient.GetAsync(path, true);
                    }
                    else
                    {
                        response = await apiClient.GetAsync(path);
                    }
                    
                    response.EnsureSuccessStatusCode();

                    var attachmentInfoResponse =
                        JsonConvert.DeserializeObject<AttachmentInfoResponseWrapper>(await response.Content.ReadAsStringAsync());

                    if (attachmentInfoResponse.AttachmentInfo.Count == 0)
                    {
                        yield break;
                    }

                    foreach (var company in attachmentInfoResponse.AttachmentInfo)
                    {
                        var recordMap = new Dictionary<string, object>();

                        foreach (var field in company)
                        {
                            recordMap[field.Key] = field.Value?.ToString() ?? "";
                        }

                        yield return new Record
                        {
                            Action = Record.Types.Action.Upsert,
                            DataJson = JsonConvert.SerializeObject(recordMap)
                        };
                    }
                } while (!string.IsNullOrWhiteSpace(nextPageUrl));
            }
        }

        public static readonly Dictionary<string, Endpoint> AttachmentInfoEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AttachmentInfo", new AttachmentInfoEndpoint
                {
                    Id = "AttachmentInfo",
                    Name = "AttachmentInfo",
                    BasePath = "/atservicesrest/v1.0/AttachmentInfo",
                    AllPath = "atservicesrest/v1.0/AttachmentInfo/query?search={\"filter\":[{\"op\" : \"exist\", \"field\" : \"id\" }]}",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "companyId"
                    }
                }
            }
        };
    }
}