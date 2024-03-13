using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginAutotask.API.Factory;
using PluginAutotask.API.Utility;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Read
{
    public static partial class Read
    {
        public static async IAsyncEnumerable<int> CountRecordsForRangedTicketHistory(IApiClient apiClient, Schema schema)
        {
            await SetClientAndThresholds(apiClient); // do not change thresholds

            var query = Utility.Utility.GetDefaultQueryForEntityId(Constants.EntityTicketHistory).Clone();
            var ticketsQuery = Utility.Utility.GetDefaultQueryForEntityId(schema.Id).Clone();
            ticketsQuery.MaxRecords = 500;

            ticketsQuery = Utility.Utility.ApplyDynamicDate(ticketsQuery);
            var ticketsQueryResult = await apiClient.GetAsync($"/{Constants.EntityTickets}/query?search={JsonConvert.SerializeObject(ticketsQuery)}");
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

                var queryResult = await apiClient.GetAsync($"/{Constants.EntityTicketHistory}/query/count?search={JsonConvert.SerializeObject(query)}");
                try
                {
                    ticketsQueryResult.EnsureSuccessStatusCode();
                }
                catch (Exception e)
                {
                    Logger.Error(e, e.Message);
                    throw;
                }

                var queryWrapper = JsonConvert.DeserializeObject<QueryCountWrapper>(await queryResult.Content.ReadAsStringAsync());
                yield return queryWrapper.QueryCount;
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

                    var queryResult = await apiClient.GetAsync($"/{Constants.EntityTicketHistory}/query/count?search={JsonConvert.SerializeObject(query)}");
                    try
                    {
                        ticketsQueryResult.EnsureSuccessStatusCode();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, e.Message);
                        throw;
                    }

                    var queryWrapper = JsonConvert.DeserializeObject<QueryCountWrapper>(await queryResult.Content.ReadAsStringAsync());
                    yield return queryWrapper.QueryCount;
                }
            }
        }
    }
}