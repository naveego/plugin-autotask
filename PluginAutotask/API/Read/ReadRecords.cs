using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginAutotask.API.Factory;
using PluginAutotask.DataContracts;
using Timer = System.Timers.Timer;

namespace PluginAutotask.API.Read
{
    public static partial class Read
    {
        static IApiClient? ApiClient = null;
        static Timer? ReadTimer = null;
        static TaskCompletionSource<bool> ReadTcs = new TaskCompletionSource<bool>();
        static int ApiDelayThreshold = 5000;
        static int ApiDelayIntervalSeconds = 300;

        public static async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient, Schema schema, int limit = -1, 
            UserDefinedQuery? userDefinedQuery = null, int apiDelayThreshold = 5000, int apiDelayIntervalSeconds = 300) 
        {
            ApiClient = apiClient;
            ReadTcs = new TaskCompletionSource<bool>();
            ApiDelayThreshold = apiDelayThreshold;
            ApiDelayIntervalSeconds = apiDelayIntervalSeconds;

            if (ReadTimer == null)
            {
                ReadTimer = new Timer();
                ReadTimer.Elapsed += new ElapsedEventHandler(OnApiUsageTimer);
            }
            
            ReadTimer.Interval = ApiDelayIntervalSeconds * 1000;
            ReadTimer.Enabled = true;

            var thresholdWrapper = await CheckApiUsage();

            if (thresholdWrapper.CurrentTimeframeRequestCount > ApiDelayThreshold) 
            {
                ReadTcs.SetCanceled();
            }

            while (ReadTcs.Task.IsCanceled)
            {
                Thread.Sleep(ApiDelayIntervalSeconds * 1000);
            }

            if (schema.Id == "TicketHistory")
            {
                var records = ReadRecordsTicketHistoryAsync(apiClient, schema, limit);
                
                await foreach (var record in records)
                {
                    yield return record;
                }
            }
            else
            {
                var entityId = schema.Id;
                var query = Utility.Utility.GetDefaultQueryForEntityId(schema.Id);

                if (userDefinedQuery != null) 
                {
                    entityId = userDefinedQuery.EntityId;
                    query = userDefinedQuery.Query;
                }

                if (limit >= 0) 
                {
                    query.MaxRecords = Math.Min(limit, 500);
                }

                var queryResult = await apiClient.GetAsync($"/{entityId}/query?search={JsonConvert.SerializeObject(query)}");
                    
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
                    while (ReadTcs.Task.IsCanceled)
                    {
                        Thread.Sleep(ApiDelayIntervalSeconds * 1000);
                    }

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
            ReadTimer.Enabled = false;
        }

        private static Record ConvertRawRecordToRecord(Dictionary<string, object> rawRecord, Schema schema)
        {
            var recordMap = new Dictionary<string, object?>();

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

        private static async Task<ThresholdWrapper> CheckApiUsage() 
        {
            if (ApiClient == null)
            {
                throw new Exception("Api Client is not set. Unable to check thresholds");
            }

            var thresholdResult = await ApiClient.GetAsync("ThresholdInformation");

            try
            {
                thresholdResult.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }

            return JsonConvert.DeserializeObject<ThresholdWrapper>(await thresholdResult.Content.ReadAsStringAsync());
        }

        private static async void OnApiUsageTimer(object source, ElapsedEventArgs eventArgs)
        {
            if (ApiClient == null)
            {
                throw new Exception("Api Client is not set. Unable to check thresholds");
            }

            var thresholdWrapper = await CheckApiUsage();

            if (thresholdWrapper.CurrentTimeframeRequestCount > ApiDelayThreshold) 
            {
                if (!ReadTcs.Task.IsCanceled)
                {
                    ReadTcs.SetCanceled();
                }
            }
            else
            {
                if (ReadTcs.Task.IsCanceled)
                {
                    ReadTcs = new TaskCompletionSource<bool>();
                }
            }
        }
    }
}