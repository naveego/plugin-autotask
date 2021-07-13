namespace PluginHubspot.API.Utility
{
    public static class Constants
    {
        //Example: "https://webservices1.autotask.net/atservicesrest/v1.0/Companies/query?search={ "filter":[{"op" : "exist", "field" : "id" }]}"
        
        //webservices1 must become user input
        public static string HttpsPrefix = "https://";
        public static string DomainApiUrl = "autotask.net/";
        public static string TestConnectionPath = "/atservicesrest/v1.0/Companies/0";
        public static string CustomProperty = "CustomProperty";
        public static string EmptySchemaDescription = "This schema has no properties. This is likely due to to there being no data.";
        
    }
}