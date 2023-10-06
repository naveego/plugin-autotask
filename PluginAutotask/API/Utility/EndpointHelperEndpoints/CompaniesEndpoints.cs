using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginHubspot.API.Factory;
using PluginHubspot.DataContracts;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public class CompaniesEndpointHelper
    {
        private class PageDetails
        {
            [JsonProperty("nextPageUrl")] public string NextPageUrl { get; set; }
            [JsonProperty("count")] public string Count { get; }
        }

        private class CompaniesResponseWrapper
        {
            [JsonProperty("items")] public List<Dictionary<string, object>> Companies { get; set; }
            [JsonProperty("pageDetails")] public PageDetails PageDetails { get; }
        }
        private class CompaniesResponse
        {
            [JsonProperty("companies")] public List<Company> Companies { get; set; }
        }

        private class Company
        {
            [JsonProperty("properties")] public Dictionary<string, CompanyProperty> Properties { get; set; }
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
        
        private class CompanyProperty
        {
            [JsonProperty("value")] public object Value { get; set; }
        }
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

        private class CompaniesEndpoint : Endpoint
        {
            private const string CompanyPropertiesPath = "atservicesrest/v1.0/Companies/entityinformation/fields";

            private const string StaticCompanyUDFPath = "atservicesrest/v1.0/Companies/query?search={\"MaxRecords\":1, " +
                                                  "\"filter\":[{\"op\":\"exist\",\"field\":\"id\"}]}";
            
            public override bool ShouldGetStaticSchema { get; set; } = true;

            // public async Task<Schema> GetStaticSchemaAsync2(IApiClient apiClient, Schema schema)
            // {
            //     // invoke companies properties api
            //     
            //     var response = await apiClient.GetAsync(CompanyPropertiesPath);
            //
            //     var endpointPropertyMetadataWrapper =
            //         JsonConvert.DeserializeObject<EndpointPropertyMetadataWrapper>(
            //             await response.Content.ReadAsStringAsync());
            //
            //     var properties = new List<Property>();
            //
            //     foreach (var companyProperty in endpointPropertyMetadataWrapper.Fields)
            //     {
            //         properties.Add(new Property
            //         {
            //             Id = companyProperty.Name,
            //             Name = companyProperty.Name,
            //             Description = "",
            //             Type = Discover.Discover.GetPropertyType(companyProperty.Type),
            //             TypeAtSource = companyProperty.Type,
            //             IsKey = Boolean.Parse(companyProperty.IsKey),
            //             IsNullable = !Boolean.Parse(companyProperty.IsRequired),
            //             IsCreateCounter = false,
            //             IsUpdateCounter = false,
            //         });
            //     }
            //     
            //     var udfResponse = await apiClient.GetAsync(StaticCompanyUDFPath);
            //
            //     udfResponse.EnsureSuccessStatusCode();
            //     
            //     var udfPropertyWrapper = JsonConvert.DeserializeObject<UDFPropertyWrapper>(await udfResponse.Content.ReadAsStringAsync());
            //
            //     foreach (var udfField in udfPropertyWrapper.Items[0].UserDefinedFields)
            //     {
            //         try
            //         {
            //             properties.Add(new Property
            //             {
            //                 Id = udfField.Name,
            //                 Name = udfField.Name,
            //                 Description = "UserDefinedField",
            //                 Type = PropertyType.String,
            //                 TypeAtSource = "string",
            //                 IsKey = false,
            //                 IsNullable = true,
            //                 IsCreateCounter = false,
            //                 IsUpdateCounter = false,
            //             });
            //         }
            //         catch (Exception e)
            //         {
            //             var debug = e.Message;
            //         }
            //         
            //     }
            //     
            //     schema.Properties.Clear();
            //     schema.Properties.AddRange(properties);
            //
            //     return schema;
            // }

            public async Task<string> WriteRecordAsync(IApiClient apiClient, Schema schema, Record record,
                IServerStreamWriter<RecordAck> responseStream)
            {
                //Get UDF fields for schema
                List<string> UDFFields = new List<string>();

                foreach (var field in schema.Properties)
                {
                    if (field.Description == "UserDefinedField")
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
                    string userDefinedFieldElement = "";
                    
                    userDefinedFieldElement += "[";
                    foreach (var field in payloadUDF)
                    {
                        userDefinedFieldElement += "{";
                        
                        userDefinedFieldElement += $"\"Name\": \"{field.Key}\",";
                        userDefinedFieldElement += $"\"Value\": \"{field.Value}\"";
                        
                        userDefinedFieldElement += "},";
                    }
                    userDefinedFieldElement = userDefinedFieldElement.TrimEnd(',');
                    userDefinedFieldElement += "]";
                    
                    payload.Add("userDefinedFields", userDefinedFieldElement);
                }
                var payloadString = JsonConvert.SerializeObject(payload);

                //Remove some serialize side-effects
                payloadString = payloadString.Replace("\"[", "[");
                payloadString = payloadString.Replace("]\"", "]");
                payloadString = payloadString.Replace("\\","");
                
                var response = await apiClient.PatchAsync(BasePath, payloadString);

                return response.StatusCode.ToString();
            }
            
            // public async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
            //     DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            // {
            //     string path = $"{AllPath.TrimStart('/')}";
            //     string nextPageUrl = "";
            //     
            //     do
            //     {
            //         var response = new HttpResponseMessage();
            //         if (!string.IsNullOrWhiteSpace(nextPageUrl))
            //         {
            //             path = nextPageUrl;
            //             response = await apiClient.GetAsync(path, true);
            //         }
            //         else
            //         {
            //             response = await apiClient.GetAsync(path);
            //         }
            //         
            //         response.EnsureSuccessStatusCode();
            //
            //         var companiesResponse =
            //             JsonConvert.DeserializeObject<CompaniesResponseWrapper>(await response.Content.ReadAsStringAsync());
            //
            //         if (companiesResponse.Companies.Count == 0)
            //         {
            //             yield break;
            //         }
            //
            //         foreach (var company in companiesResponse.Companies)
            //         {
            //             var recordMap = new Dictionary<string, object>();
            //
            //             foreach (var field in company)
            //             {
            //                 if (field.Key != "userDefinedFields")
            //                 {
            //                     recordMap[field.Key] = field.Value?.ToString() ?? "";
            //                 }
            //                 else
            //                 {
            //                     if (!string.IsNullOrWhiteSpace(field.Value.ToString()))
            //                     {
            //                         var udfFields = JsonConvert.DeserializeObject<List<UDFListItemRootless>>(field.Value.ToString());
            //
            //                         foreach (var udfField in udfFields)
            //                         {
            //                             recordMap[udfField.Name] = udfField.Value?.ToString() ?? "";
            //                         }
            //
            //                     }
            //                     //deserialize field.value
            //                 }
            //             }
            //
            //             
            //             yield return new Record
            //             {
            //                 Action = Record.Types.Action.Upsert,
            //                 DataJson = JsonConvert.SerializeObject(recordMap)
            //             };
            //         }
            //     } while (!string.IsNullOrWhiteSpace(nextPageUrl));
            // }
        }

        public static readonly Dictionary<string, Endpoint> CompaniesEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "Companies", new CompaniesEndpoint
                {
                    Id = "Companies",
                    Name = "Companies",
                    BasePath = "/atservicesrest/v1.0/Companies",
                    AllPath = "atservicesrest/v1.0/Companies/query?search={\"filter\":[{\"op\" : \"exist\", \"field\" : \"id\" }]}",
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