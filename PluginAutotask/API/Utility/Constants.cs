using System.Collections.Generic;

namespace PluginAutotask.API.Utility
{
    public static class Constants
    {
        public static string TestConnectionPath = "Companies/0";
        public static string GetAllRecordsQuery = "{\"Filter\":[{\"field\":\"Id\",\"op\":\"gte\",\"value\":0}]}";
        public static string UserDefinedProperty = "UserDefinedProperty";
        public static string EmptySchemaDescription = "This schema has no properties. This is likely due to to there being no data.";

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