using System;
using System.Threading.Tasks;
using Aunalytics.Sdk.Plugins;
using PluginAutotask.API.Factory;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Discover
{
    public static partial class Discover
    {
        public static Task<Count> GetCountOfRecordsRangedTicketHistory(IApiClient apiClient, Schema schema, UserDefinedQuery? userDefinedQuery = null)
        {
            throw new NotImplementedException();
        }
    }
}