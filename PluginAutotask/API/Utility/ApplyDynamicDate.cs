using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Utility
{
    public static partial class Utility
    {
        public static string DynamicDateConstant = "TODAYMINUS_";
        public static string DynamicDatePattern = @"^TODAYMINUS_[0-9]{1,3}_DAYS$";

        public static Query ApplyDynamicDate(Query query)
        {
            if (!JsonConvert.SerializeObject(query.Filter).Contains(DynamicDateConstant))
            {
                return query;
            }

            foreach (var filter in query.Filter)
            {
                if (filter.Value is string)
                {
                    var value = (string)filter.Value;
                    var parts = value.Split('_');
                    if (parts.Length != 3 || !int.TryParse(parts[1], out var numDays) || !Regex.IsMatch(value, DynamicDatePattern))
                    {
                        throw new Exception($"Invalid dynamic date format given. Expected: 'TODAYMINUS_N_DAYS', Got: '{value}'");
                    }

                    numDays = int.Parse(parts[1]);
                    var dynamicDate = DateTime.Today.AddDays(-(numDays));
                    var dynamicFilterValue = dynamicDate.ToString("yyyy-MM-dd");
                    filter.Value = dynamicFilterValue;
                }
            }

            return query;
        }
    }
}