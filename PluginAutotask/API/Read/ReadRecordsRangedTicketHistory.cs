using System;
using System.Collections.Generic;
using Aunalytics.Sdk.Plugins;
using PluginAutotask.API.Factory;

namespace PluginAutotask.API.Read
{
    public static partial class Read
    {
        public static IAsyncEnumerable<Record> ReadRecordsRangedTicketHistoryAsync(IApiClient apiClient, Schema schema, int limit = -1) 
        {
            throw new NotImplementedException();
        }
    }
}