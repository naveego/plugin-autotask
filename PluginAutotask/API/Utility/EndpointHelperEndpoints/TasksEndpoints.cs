using System;
using System.Collections.Generic;
using System.IO;
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
using PluginHubspot.Helper;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public class TasksEndpointHelper
    {
        private class PageDetails
        {
            [JsonProperty("nextPageUrl")] public string NextPageUrl { get; }
            [JsonProperty("count")] public string Count { get; }
        }
        private class TasksResponseWrapper
        {
            [JsonProperty("items")] public List<Dictionary<string, object>> Tasks { get; set; }
            [JsonProperty("pageDetails")] public PageDetails PageDetails { get; }
        }
        private class TasksResponse
        {
            [JsonProperty("tasks")] public List<Task> Tasks { get; set; }

        }

        private class Task
        {
            [JsonProperty("properties")] public Dictionary<string, TaskProperty> Properties { get; set; }
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
        private class TaskProperty
        {
            [JsonProperty("value")] public object Value { get; set; }
        }

        private class TaskPropertyMetadataWrapper
        {
            [JsonProperty("fields")] public List<TaskPropertyMetadata> Fields { get; set; }
        }
        private class TaskPropertyMetadata
        {
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("isReference")] public string IsKey { get; set; }
            [JsonProperty("isRequired")] public string IsRequired { get; set; }
            [JsonProperty("dataType")] public string Type { get; set; }
        }

        private class TasksEndpoint : Endpoint
        {
            private const string TaskPropertiesPath = "atservicesrest/v1.0/Tasks/entityinformation/fields";
            
            //This is the schema to emulate

            public override bool ShouldGetStaticSchema { get; set; } = true;

            public async Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                // invoke tasks properties api
                var response = await apiClient.GetAsync(TaskPropertiesPath);

                var taskPropertyWrapper =
                    JsonConvert.DeserializeObject<TaskPropertyMetadataWrapper>(
                        await response.Content.ReadAsStringAsync());
                
                var properties = new List<Property>();

                // foreach (var taskProperty in taskProperties)
                foreach (var taskProperty in taskPropertyWrapper.Fields)
                {
                    properties.Add(new Property
                    {
                        Id = taskProperty.Name,
                        Name = taskProperty.Name,
                        Description = "",
                        Type = Discover.Discover.GetPropertyType(taskProperty.Type),
                        TypeAtSource = taskProperty.Type,
                        IsKey = Boolean.Parse(taskProperty.IsKey),
                        IsNullable = !Boolean.Parse(taskProperty.IsRequired),
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                    });
                    schema.Properties.AddRange(properties);
                }

                return schema;
            }

            public async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
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

                    var tasksResponse =
                        JsonConvert.DeserializeObject<TasksResponseWrapper>(await response.Content.ReadAsStringAsync());

                    if (tasksResponse.Tasks.Count == 0)
                    {
                        yield break;
                    }

                    foreach (var task in tasksResponse.Tasks)
                    {
                        
                        var recordMap = new Dictionary<string, object>();

                        foreach (var field in task)
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

        public static readonly Dictionary<string, Endpoint> TasksEndpoints = new Dictionary<string, Endpoint>
        {
            {
                //autotask.net/atservicesrest/v1.0/Tasks/query?search={"filter":[{"op":"eq","field":"CompanyID","value":175}]}
                "Tasks", new TasksEndpoint
                {
                    Id = "Tasks",
                    Name = "Tasks",
                    BasePath = "/atservicesrest/v1.0/Tasks",
                    AllPath = "atservicesrest/v1.0/Tasks/query?search={\"filter\":[{\"op\" : \"exist\", \"field\" : \"id\" }]}",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get,
                        EndpointActions.Patch,
                        EndpointActions.Post,
                        EndpointActions.Put
                    },
                    
                 }
            }
        };
    }
}