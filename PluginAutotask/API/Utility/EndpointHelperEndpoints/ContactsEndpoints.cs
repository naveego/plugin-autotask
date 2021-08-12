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
    public class ContactsEndpointHelper
    {
        private class PageDetails
        {
            [JsonProperty("nextPageUrl")] public string NextPageUrl { get; set; }
            [JsonProperty("count")] public string Count { get; set;}
        }

        private class ContactsResponseWrapper
        {
            [JsonProperty("items")] public List<Dictionary<string, object>> Contacts { get; set; }
            [JsonProperty("pageDetails")] public PageDetails PageDetails { get; set;}
        }
        private class ContactsResponse
        {
            [JsonProperty("contacts")] public List<Contact> Contacts { get; set; }
        }

        private class Contact
        {
            [JsonProperty("properties")] public Dictionary<string, ContactProperty> Properties { get; set; }
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
        private class ContactProperty
        {
            [JsonProperty("value")] public object Value { get; set; }
        }

        private class ContactPropertyMetadataWrapper
        {
            [JsonProperty("fields")] public List<ContactPropertyMetadata> Fields { get; set; }
        }
        private class ContactPropertyMetadata
        {
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("isReference")] public string IsKey { get; set; }
            [JsonProperty("isRequired")] public string IsRequired { get; set; }
            [JsonProperty("dataType")] public string Type { get; set; }
        }

        private class ContactsEndpoint : Endpoint
        {
            private const string ContactPropertiesPath = "atservicesrest/v1.0/Contacts/entityinformation/fields";

            private List<string> ContactRequiredFields = new List<string>()
            {
                "id",
                "companyID",
                "firstName",
                "lastName",
                "isActive"
            };

            public async Task<bool> ValidateJSON(Dictionary<string, string> dataJson)
            {
                foreach (var field in ContactRequiredFields)
                {
                    if (!dataJson.ContainsKey(field))
                    {
                        throw new Exception($"Required Contact field {field} not found in JSON patch request.");
                    }

                    if (String.IsNullOrWhiteSpace(dataJson[field]))
                    {
                        throw new Exception($"Required Contact field {field} is null.");   
                    }
                }

                return true;
            }
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

        public static readonly Dictionary<string, Endpoint> ContactsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "Contacts", new ContactsEndpoint
                {
                    Id = "Contacts",
                    Name = "Contacts",
                    BasePath = "/atservicesrest/v1.0/Contacts",
                    AllPath = "atservicesrest/v1.0/Contacts/query?search={\"filter\":[{\"op\" : \"exist\", \"field\" : \"id\" }]}",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    
                 }
            }
        };
    }
}