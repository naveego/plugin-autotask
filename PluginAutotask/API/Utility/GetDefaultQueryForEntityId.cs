using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Utility
{
    public static partial class Utility
    {
        public static Query GetDefaultQueryForEntityId(string entityId)
        {
            switch (entityId)
            {
                case Constants.EntityTicketHistory:
                    return Constants.GetAllRecordsQueryTicketHistory;
                case Constants.EntityTicketHistoryLast05:
                    return Constants.RangedTicketQueryPrev05Days;
                case Constants.EntityTicketHistoryLast30:
                    return Constants.RangedTicketQueryPrev30Days;
                default:
                    return Constants.GetAllRecordsQuery;
            }
        }
    }
}