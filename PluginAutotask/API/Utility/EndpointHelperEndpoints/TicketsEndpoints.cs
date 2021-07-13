using System;
using System.Collections.Generic;
using System.IO;
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
using PluginHubspot.Helper;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public class TicketsEndpointHelper
    {
        private class PageDetails
        {
            [JsonProperty("nextPageUrl")] public string NextPageUrl { get; }
            [JsonProperty("count")] public string Count { get; }
        }
        private class TicketsResponseWrapper
        {
            [JsonProperty("items")] public List<Dictionary<string, object>> Tickets { get; set; }
            [JsonProperty("pageDetails")] public PageDetails PageDetails { get; }
        }
        private class TicketsResponse
        {
            [JsonProperty("tickets")] public List<Ticket> Tickets { get; set; }
        }

        private class Ticket
        {
            [JsonProperty("properties")] public Dictionary<string, TicketProperty> Properties { get; set; }
        }

        private class TicketProperty
        {
            [JsonProperty("value")] public object Value { get; set; }
        }

        private class TicketPropertyMetadataWrapper
        {
            [JsonProperty("fields")] public List<TicketPropertyMetadata> Fields { get; set; }
        }
        private class TicketPropertyMetadata
        {
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("isReference")] public string IsKey { get; set; }
            [JsonProperty("isRequired")] public string IsRequired { get; set; }
            [JsonProperty("dataType")] public string Type { get; set; }
        }
        private class TicketPostBody
        {
            [JsonProperty("id")] public String Id { get; set; }
            [JsonProperty("attachDate")] public String AttachDate { get; set; }
            [JsonProperty("attachedByContactID")] public String AttachmentByContactId { get; set; }
            [JsonProperty("attachedByResourceID")] public String AttachmentByResourceId { get; set; }
            [JsonProperty("attachmentType")] public String AttachmentType { get; set; }
            [JsonProperty("fullPath")] public String FullPath { get; set; }
            [JsonProperty("publish")] public String Publish { get; set; }
            [JsonProperty("title")] public String Title { get; set; }
            [JsonProperty("data")] public String Data { get; set; }
        }
        
        private class TicketsEndpoint : Endpoint
        {
            private const string TicketPropertiesPath = "atservicesrest/v1.0/Tickets/entityinformation/fields";
            
            public override bool ShouldGetStaticSchema { get; set; } = true;

            public async Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                
                throw new NotImplementedException();
            }

            public async Task<string> WriteRecordAsync(IApiClient apiClient, Schema schema, Record record,
                IServerStreamWriter<RecordAck> responseStream)
            {
                
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                throw new NotImplementedException();
            }
        }

        public static readonly Dictionary<string, Endpoint> TicketsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "Tickets", new TicketsEndpoint
                {
                    Id = "Tickets",
                    Name = "Tickets",
                    BasePath = "/atservicesrest/v1.0/Tickets",
                    AllPath = "atservicesrest/v1.0/Tickets/query?search={\"filter\":[{\"op\" : \"exist\", \"field\" : \"id\" }]}",
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