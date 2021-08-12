using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginHubspot.API.Factory;
using PluginHubspot.DataContracts;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public class ProjectsEndpointHelper
    {
        private class PageDetails
        {
            [JsonProperty("nextPageUrl")] public string NextPageUrl { get; set; }
            [JsonProperty("count")] public string Count { get; }
        }

        private class ProjectsResponseWrapper
        {
            [JsonProperty("items")] public List<Dictionary<string, object>> Projects { get; set; }
            [JsonProperty("pageDetails")] public PageDetails PageDetails { get; }
        }
        private class ProjectsResponse
        {
            [JsonProperty("projects")] public List<Project> Projects { get; set; }
        }

        private class Project
        {

            [JsonProperty("properties")] public Dictionary<string, ProjectProperty> Properties { get; set; }
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
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("value")]
            public string Value { get; set; }
        }
        
        private class UDFListItemRootless
        {
            public string Name { get; set; }
            
            public string Value { get; set; }
        }
        private class ProjectProperty
        {
            [JsonProperty("value")] public object Value { get; set; }
        }
        private class ProjectPropertyMetadataWrapper
        {
            [JsonProperty("fields")] public List<ProjectPropertyMetadata> Fields { get; set; }
        }
        private class ProjectPropertyMetadata
        {
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("isReference")] public string IsKey { get; set; }
            [JsonProperty("isRequired")] public string IsRequired { get; set; }
            [JsonProperty("dataType")] public string Type { get; set; }
        }

        private class ProjectsEndpoint : Endpoint
        {
            private const string ProjectPropertiesPath = "atservicesrest/v1.0/Projects/entityinformation/fields";

            public override bool ShouldGetStaticSchema { get; set; } = true;

            public async Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                // invoke projects properties api
                var response = await apiClient.GetAsync(ProjectPropertiesPath);

                var companyPropertyWrapper =
                    JsonConvert.DeserializeObject<ProjectPropertyMetadataWrapper>(
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

            public IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                throw new NotImplementedException();
            }
        }

        public static readonly Dictionary<string, Endpoint> ProjectsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "Projects", new ProjectsEndpoint
                {
                    Id = "Projects",
                    Name = "Projects",
                    BasePath = "/atservicesrest/v1.0/Projects",
                    AllPath = "atservicesrest/v1.0/Projects/query?search={\"filter\":[{\"op\" : \"exist\", \"field\" : \"id\" }]}",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get,
                        EndpointActions.Patch,
                        EndpointActions.Post,
                        EndpointActions.Put
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