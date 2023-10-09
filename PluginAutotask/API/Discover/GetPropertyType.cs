using Aunalytics.Sdk.Plugins;

namespace PluginAutotask.API.Discover
{
    public static partial class Discover
    {
        public static PropertyType GetPropertyType(string? dataType)
        {
            if (dataType == null)
            {
                return PropertyType.String;
            }

            switch (dataType)
            {
                case "datetime":
                    return PropertyType.Datetime;
                case "date":
                    return PropertyType.Date;
                case "time":
                    return PropertyType.Time;
                case "integer":
                case "long":
                    return PropertyType.Integer;
                case "decimal":
                    return PropertyType.Decimal;
                case "float":
                case "double":
                    return PropertyType.Float;
                case "boolean":
                    return PropertyType.Bool;
                case var t when t.Contains("string"):
                    return PropertyType.String;
                default:
                    return PropertyType.String;
            }
        }
    }
}