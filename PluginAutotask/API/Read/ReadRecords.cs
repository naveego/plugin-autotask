using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginAutotask.API.Factory;
using PluginAutotask.API.Utility;
using PluginAutotask.DataContracts;
using PluginAutotask.Helper;

namespace PluginAutotask.API.Read
{
    public static partial class Read
    {
        public static async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient, Schema schema) 
        {
            var queryResult = await apiClient.GetAsync($"/{schema.Id}/query?search={Constants.GetAllRecordsQuery}");
                
            try
            {
                queryResult.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
            
            var queryWrapper = JsonConvert.DeserializeObject<QueryWrapper>(await queryResult.Content.ReadAsStringAsync());
            foreach (var rawRecord in queryWrapper.Items)
            {
                yield return ConvertRawRecordToRecord(rawRecord, schema);
            }
            
            while (queryWrapper.PageDetails.NextPageUrl != null)
            {
                queryResult = await apiClient.GetAsync(queryWrapper.PageDetails.NextPageUrl);

                try
                {
                    queryResult.EnsureSuccessStatusCode();
                }
                catch (Exception e)
                {
                    Logger.Error(e, e.Message);
                    throw;
                }
                
                queryWrapper = JsonConvert.DeserializeObject<QueryWrapper>(await queryResult.Content.ReadAsStringAsync());
                foreach (var rawRecord in queryWrapper.Items)
                {
                    yield return ConvertRawRecordToRecord(rawRecord, schema);
                }
            }
        }

        private static Record ConvertRawRecordToRecord(Dictionary<string, object> rawRecord, Schema schema)
        {
            var recordMap = new Dictionary<string, object>();

            foreach (var property in schema.Properties)
            {
                try
                {
                    if (rawRecord.ContainsKey(property.Id))
                    {
                        if (rawRecord[property.Id] == null)
                        {
                            recordMap[property.Id] = null;
                        }
                        else
                        {
                            switch (property.Type)
                            {
                                case PropertyType.String:
                                case PropertyType.Text:
                                case PropertyType.Decimal:
                                    recordMap[property.Id] = rawRecord[property.Id].ToString();
                                    break;
                                default:
                                    recordMap[property.Id] = rawRecord[property.Id];
                                    break;
                            }
                        }
                    }
                    else
                    {
                        recordMap[property.Id] = null;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"No column with property Id: {property.Id}");
                    Logger.Error(e, e.Message);
                    recordMap[property.Id] = null;
                }
            }

            return new Record() 
            {
                Action = Record.Types.Action.Upsert,
                DataJson = JsonConvert.SerializeObject(recordMap),
            };
        }
    }
}