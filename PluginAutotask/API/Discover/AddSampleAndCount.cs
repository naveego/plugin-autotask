using System.Linq;
using System.Threading.Tasks;
using Aunalytics.Sdk.Plugins;
using PluginAutotask.API.Factory;
using PluginAutotask.DataContracts;

namespace PluginAutotask.API.Discover
{
    public static partial class Discover
    {
        public static async Task<Schema> AddSampleAndCount(IApiClient apiClient, Schema schema, int sampleSize, UserDefinedQuery? userDefinedQuery = null)
        {
            // add sample and count
            var records = Read.Read.ReadRecordsAsync(apiClient, schema, sampleSize, userDefinedQuery).Take(sampleSize);
            schema.Sample.AddRange(await records.ToListAsync());
            schema.Count = await GetCountOfRecords(apiClient, schema, userDefinedQuery);

            return schema;
        }
    }
}