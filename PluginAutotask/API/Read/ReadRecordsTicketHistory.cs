using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginAutotask.API.Factory;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Read
{
    public static partial class Read
    {
        public static async IAsyncEnumerable<Record> ReadRecordsTicketHistoryAsync(IApiClient apiClient, Schema schema, int limit = -1) 
        {
            var query = Utility.Utility.GetQueryForSchemaId(schema.Id);
            var ticketsQuery = Utility.Utility.GetQueryForSchemaId("Tickets");
            if (limit >= 0) 
            {
                ticketsQuery.MaxRecords = Math.Min(limit, 500);
            }

            var ticketsQueryResult = await apiClient.GetAsync($"/Tickets/query?search={JsonConvert.SerializeObject(ticketsQuery)}");
            try
            {
                ticketsQueryResult.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
            
            var ticketsQueryWrapper = JsonConvert.DeserializeObject<QueryWrapper>(await ticketsQueryResult.Content.ReadAsStringAsync());
            foreach (var rawTicketRecord in ticketsQueryWrapper.Items)
            {
                while (ReadTcs.Task.IsCanceled)
                {
                    Thread.Sleep(ApiDelayIntervalSeconds * 1000);
                }

                var ticketId = rawTicketRecord["id"];
                query.Filter.First().Value = ticketId;
                
                var queryResult = await apiClient.GetAsync($"/{schema.Id}/query?search={JsonConvert.SerializeObject(query)}");
                try
                {
                    ticketsQueryResult.EnsureSuccessStatusCode();
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
            }
            
            while (ticketsQueryWrapper.PageDetails.NextPageUrl != null)
            {
                while (ReadTcs.Task.IsCanceled)
                {
                    Thread.Sleep(ApiDelayIntervalSeconds * 1000);
                }

                ticketsQueryResult = await apiClient.GetAsync(ticketsQueryWrapper.PageDetails.NextPageUrl);
                try
                {
                    ticketsQueryResult.EnsureSuccessStatusCode();
                }
                catch (Exception e)
                {
                    Logger.Error(e, e.Message);
                    throw;
                }
                
                ticketsQueryWrapper = JsonConvert.DeserializeObject<QueryWrapper>(await ticketsQueryResult.Content.ReadAsStringAsync());
                foreach (var rawTicketRecord in ticketsQueryWrapper.Items)
                {
                    while (ReadTcs.Task.IsCanceled)
                    {
                        Thread.Sleep(ApiDelayIntervalSeconds * 1000);
                    }

                    var ticketId = rawTicketRecord["id"];
                    query.Filter.First().Value = ticketId;
                    
                    var queryResult = await apiClient.GetAsync($"/{schema.Id}/query?search={JsonConvert.SerializeObject(query)}");
                    try
                    {
                        ticketsQueryResult.EnsureSuccessStatusCode();
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
                }
            }
        }
    }
}