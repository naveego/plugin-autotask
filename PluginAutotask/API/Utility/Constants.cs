using System.Collections.Generic;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Utility
{
    public static class Constants
    {
        public static string TestConnectionPath = "Companies/0";
        public static Query GetAllRecordsQuery = new Query() 
        {
            Filter = new List<Filter>()
            {
                new Filter()
                {
                    Field = "id",
                    Operation = "gte",
                    Value = 0
                }
            }
        };

        public static Query GetAllRecordsQueryTicketHistory = new Query() 
        {
            Filter = new List<Filter>()
            {
                new Filter()
                {
                    Field = "ticketID",
                    Operation = "eq",
                    Value = 0
                }
            }
        };

        public static string UserDefinedProperty = "User defined property";

        public static List<string> EntitiesList { get; set; } = new List<string>() {
            "BillingCodes",
            "BillingItems",
            "ContactBillingProductAssociations",
            "ContactGroupContacts",
            "ContactGroups",
            "ContractBillingRules",
            "ContractCharges",
            "ContractRates",
            "ContractRoleCosts",
            "ContractServiceBundles",
            "ContractServiceBundleUnits",
            "ContractServices",
            "ContractServiceUnits",
            "Holidays",
            "HolidaySets",
            "Invoices",
            "Products",
            "ResourceDailyAvailabilities",
            "ResourceRoleQueues",
            "ResourceRoles",
            "Resources",
            "ResourceServiceDeskRoles",
            "Roles",
            "ServiceBundles",
            "ServiceBundleServices",
            "ServiceLevelAgreementResults",
            "Services",
            "SubscriptionPeriods",
            "Subscriptions",
            "SurveyResults",
            "Surveys",
            "TaskSecondaryResources",
            "TicketCategories",
            "TicketCharges",
            "TicketHistory",
            "TicketNotes",
            "TimeEntries",
            "UserDefinedFieldDefinitions",
            "UserDefinedFieldListItems",
        };
    }
}