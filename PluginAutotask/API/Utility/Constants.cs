using System.Collections.Generic;
using System.Linq;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Utility
{
    public static class Constants
    {
        public const string EntityTickets = "Tickets";
        public const string EntityTicketHistory = "TicketHistory";
        public const string EntityTicketHistoryLast05 = "TicketHistoryLast05Days";
        public const string EntityTicketHistoryLast30 = "TicketHistoryLast30Days";

        public static string TestConnectionPath = "Companies/0";

        public static readonly string[] RangedTicketHistoryNames = new string[]
        {
            EntityTicketHistoryLast05,
            EntityTicketHistoryLast30,
        };

        public static Query GetAllRecordsQuery = new Query()
        {
            Filter = new List<Filter>
            {
                new Filter
                {
                    Field = "id",
                    Operation = "gte",
                    Value = 0
                }
            }
        };

        public static Query GetAllRecordsQueryTicketHistory = new Query()
        {
            Filter = new List<Filter>
            {
                new Filter
                {
                    Field = "ticketID",
                    Operation = "eq",
                    Value = 0
                }
            }
        };

        public static Query RangedTicketQueryPrev05Days = new Query()
        {
            IncludeFields = new List<string>
            {
                "id",
                "lastActivityDate"
            },
            Filter = new List<Filter>
            {
                new Filter
                {
                    Field = "lastActivityDate",
                    Operation = "gte",
                    Value = "TODAYMINUS_5_DAYS"
                }
            }
        };

        public static Query RangedTicketQueryPrev30Days = new Query()
        {
            IncludeFields = new List<string>
            {
                "id",
                "lastActivityDate"
            },
            Filter = new List<Filter>
            {
                new Filter
                {
                    Field = "lastActivityDate",
                    Operation = "gte",
                    Value = "TODAYMINUS_30_DAYS"
                }
            }
        };

        public static string UserDefinedProperty = "User defined property";

        public static List<string> EntitiesList { get; set; } = new List<string> {
            // phase 1
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
            EntityTicketHistory,
            "TicketNotes",
            "TimeEntries",
            "UserDefinedFieldDefinitions",
            "UserDefinedFieldListItems",
            // phase 2
            "CompanyAlerts",
            "CompanyLocations",
            "ContractBlockHourFactors",
            "ContractBlocks",
            "ContractMilestones",
            "DeletedTaskActivityLogs",
            "DeletedTicketActivityLogs",
            "DeletedTicketLogs",
            "Departments",
            "Opportunities",
            "OpportunityCategories",
            "OrganizationalLevel1s",
            "OrganizationalLevel2s",
            "OrganizationalLevelAssociations",
            "OrganizationalResources",
            "PaymentTerms",
            "Phases",
            "ProductTiers",
            "ProductVendors",
            "ProjectCharges",
            "Quotes",
            "ResourceRoleDepartments",
            "ResourceSkills",
            "ServiceCalls",
            "ServiceCallTaskResources",
            "ServiceCallTasks",
            "ServiceCallTicketResources",
            "ServiceCallTickets",
            "Skills",
            // DATAINT-1770
            "Companies",
            "Contacts",
            "Tasks",
            "Projects",
            // DATAINT-1780
            EntityTicketHistoryLast05,
            EntityTicketHistoryLast30
        };

        public static bool IsRangedTicketHistoryName(string entityName) =>
            RangedTicketHistoryNames.Contains(entityName);
    }
}