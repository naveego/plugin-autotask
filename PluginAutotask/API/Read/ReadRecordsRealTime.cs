using System;
using System.Threading.Tasks;
using Grpc.Core;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginHubspot.API.Factory;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Read
{
    public static partial class Read
    {
        public static async Task<long> ReadRecordsRealTimeAsync(IApiClient apiClient, Settings settings, ReadRequest request,
            IServerStreamWriter<Record> responseStream,
            ServerCallContext context)
        {
            Logger.Info("Beginning to read records real time...");
            
            var schema = request.Schema;
            var jobVersion = request.DataVersions.JobDataVersion;
            var recordsCount = 0;

            try
            {
                Logger.Info("Real time read initializing...");
                var realTimeSettings = JsonConvert.DeserializeObject<RealTimeSettings>(request.RealTimeSettingsJson);
                var realTimeState = !string.IsNullOrWhiteSpace(request.RealTimeStateJson) ?
                    JsonConvert.DeserializeObject<RealTimeState>(request.RealTimeStateJson) :
                    new RealTimeState();

                if (jobVersion > realTimeState.JobVersion)
                {
                    realTimeState.LastReadTime = DateTime.MinValue;
                }
                
                Logger.Info("Real time read initialized.");

                while (!context.CancellationToken.IsCancellationRequested)
                {
                    long currentRunRecordsCount = 0;
                    Logger.Debug($"Getting all records since {realTimeState.LastReadTime.ToUniversalTime():O}");
                    
                    var tcs = new TaskCompletionSource<DateTime>();
                    
                    var records = ReadRecordsAsync(apiClient, schema, settings, realTimeState.LastReadTime, tcs);

                    await foreach (var record in records)
                    {

                        // publish record
                        await responseStream.WriteAsync(record);
                        recordsCount++;
                        currentRunRecordsCount++;
                    }

                    realTimeState.LastReadTime = await tcs.Task;
                    realTimeState.JobVersion = jobVersion;

                    var realTimeStateCommit = new Record
                    {
                        Action = Record.Types.Action.RealTimeStateCommit,
                        RealTimeStateJson = JsonConvert.SerializeObject(realTimeState)
                    };
                    await responseStream.WriteAsync(realTimeStateCommit);
                    
                    Logger.Debug($"Got {currentRunRecordsCount} records since {realTimeState.LastReadTime.ToUniversalTime():O}");

                    await Task.Delay(realTimeSettings.PollingInterval * 1000, context.CancellationToken);
                }
            }
            catch (TaskCanceledException e)
            {
                return recordsCount;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                throw;
            }
            
            return recordsCount;
        }
    }
}