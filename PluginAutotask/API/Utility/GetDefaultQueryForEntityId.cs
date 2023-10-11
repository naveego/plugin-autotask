using System.Collections.Generic;
using Aunalytics.Sdk.Plugins;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Utility
{
    public static partial class Utility
    {
        public static Query GetDefaultQueryForEntityId(string entityId)
        {
            switch (entityId)
            {
                case "TicketHistory":
                    return Constants.GetAllRecordsQueryTicketHistory;
                default:
                    return Constants.GetAllRecordsQuery;
            }
        }
    }
}