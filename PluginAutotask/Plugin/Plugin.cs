using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Aunalytics.Sdk.Logging;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginAutotask.API.Discover;
using PluginAutotask.API.Factory;
using PluginAutotask.API.Read;
using PluginAutotask.Helper;
using PluginAutotask.DataContracts;
using PluginAutotask.API.Utility;

namespace PluginAutotask.Plugin
{
    public class Plugin : Publisher.PublisherBase
    {
        private readonly ServerStatus _server;
        private TaskCompletionSource<bool> _tcs;
        private readonly IApiClientFactory _apiClientFactory;
        private IApiClient _apiClient;

        public Plugin(HttpClient client = null)
        {
            _apiClientFactory = new ApiClientFactory(client ?? new HttpClient());
            _server = new ServerStatus
            {
                Connected = false,
                WriteConfigured = false
            };
        }
        
        /// <summary>
        /// Configures the plugin
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ConfigureResponse> Configure(ConfigureRequest request, ServerCallContext context)
        {
            Logger.Debug("Got configure request");
            Logger.Debug(JsonConvert.SerializeObject(request, Formatting.Indented));
            
            // ensure all directories are created
            Directory.CreateDirectory(request.TemporaryDirectory);
            Directory.CreateDirectory(request.PermanentDirectory);
            Directory.CreateDirectory(request.LogDirectory);
            
            // configure logger
            Logger.SetLogLevel(request.LogLevel);
            Logger.Init(request.LogDirectory);

            _server.Config = request;

            return Task.FromResult(new ConfigureResponse());
        }
        

        /// <summary>
        /// Establishes a connection with Autotask.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>A message indicating connection success</returns>
        public override async Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
        {
            // for setting the log level
            // Logger.SetLogLevel(Logger.LogLevel.Debug);

            Logger.SetLogPrefix("connect");
            
            // validate settings passed in
            try
            {
                _server.Settings = JsonConvert.DeserializeObject<Settings>(request.SettingsJson);
                _server.Settings.Validate();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = "",
                    OauthError = "",
                    SettingsError = e.Message
                };
            }

            // get api client
            try
            {
                _apiClient = _apiClientFactory.CreateApiClient(_server.Settings);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = "",
                    OauthError = "",
                    SettingsError = e.Message
                };
            }

            // test api client
            try
            {
                await _apiClient.TestConnection();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);

                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = e.Message,
                    OauthError = "",
                    SettingsError = ""
                };
            }

            _server.Connected = true;

            return new ConnectResponse
            {
                OauthStateJson = request.OauthStateJson,
                ConnectionError = "",
                OauthError = "",
                SettingsError = ""
            };
        }

        public override async Task ConnectSession(ConnectRequest request,
            IServerStreamWriter<ConnectResponse> responseStream, ServerCallContext context)
        {
            Logger.SetLogPrefix("connect_session");
            Logger.Info("Connecting session...");

            // create task to wait for disconnect to be called
            _tcs?.SetResult(true);
            _tcs = new TaskCompletionSource<bool>();

            // call connect method
            var response = await Connect(request, context);

            await responseStream.WriteAsync(response);

            Logger.Info("Session connected.");

            // wait for disconnect to be called
            await _tcs.Task;
        }


        /// <summary>
        /// Discovers schemas located in the users Campaigner instance
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Discovered schemas</returns>
        public override async Task<DiscoverSchemasResponse> DiscoverSchemas(DiscoverSchemasRequest request,
            ServerCallContext context)
        {
            Logger.SetLogPrefix("discover");
            Logger.Info("Discovering Schemas...");
            
            var sampleSize = checked((int) request.SampleSize);

            DiscoverSchemasResponse discoverSchemasResponse = new DiscoverSchemasResponse();

            if (request.Mode == DiscoverSchemasRequest.Types.Mode.All)
            {
                // get all schemas
                try
                {
                    var schemas = Discover.GetAllSchemas(_apiClient, sampleSize);

                    discoverSchemasResponse.Schemas.AddRange(await schemas.ToListAsync());

                    Logger.Info($"Schemas returned: {discoverSchemasResponse.Schemas.Count}");

                    return discoverSchemasResponse;
                }
                catch (Exception e)
                {
                    Logger.Error(e, e.Message, context);
                    return new DiscoverSchemasResponse();
                }
            }

            // only return requested schemas if refresh mode selected
            try
            {
                var refreshSchemas = request.ToRefresh;

                Logger.Info($"Refresh schemas attempted: {refreshSchemas.Count}");

                var schemas = Discover.GetRefreshSchemas(_apiClient, refreshSchemas, sampleSize);

                discoverSchemasResponse.Schemas.AddRange(await schemas.ToListAsync());

                // return all schemas 
                Logger.Info($"Schemas returned: {discoverSchemasResponse.Schemas.Count}");
                return discoverSchemasResponse;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                return new DiscoverSchemasResponse();
            }
        }
        
        /// <summary>
        /// Publishes a stream of data for a given schema
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ReadStream(ReadRequest request, IServerStreamWriter<Record> responseStream,
            ServerCallContext context)
        {
            try
            {
                var schema = request.Schema;
                var limit = checked((int) request.Limit);
                var limitFlag = request.Limit != 0;
                var jobId = request.JobId;
                long recordsCount = 0;
                UserDefinedQuery? userDefinedQuery = null;

                Logger.SetLogPrefix(jobId);

                if (!string.IsNullOrWhiteSpace(schema.Query))
                {
                    userDefinedQuery = Utility.ParseUserDefinedQuery(schema.Query);
                }
                
                var records = Read.ReadRecordsAsync(_apiClient, schema, limit, userDefinedQuery, _server.Settings.ApiUsageThreshold, _server.Settings.ApiDelayIntervalSeconds);

                await foreach (var record in records)
                {
                    // stop publishing if the limit flag is enabled and the limit has been reached or the server is disconnected
                    if (limitFlag && recordsCount == limit || !_server.Connected)
                    {
                        break;
                    }

                    // publish record
                    await responseStream.WriteAsync(record);
                    recordsCount++;
                }
                
                Logger.Info($"Published {recordsCount} records");
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
            }
        }

        /// <summary>
        /// Handles disconnect requests from the agent
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<DisconnectResponse> Disconnect(DisconnectRequest request, ServerCallContext context)
        {
            // clear connection
            _server.Connected = false;
            _server.Settings = null;

            // alert connection session to close
            if (_tcs != null)
            {
                _tcs.SetResult(true);
                _tcs = null;
            }

            Logger.Info("Disconnected");
            return Task.FromResult(new DisconnectResponse());
        }
    }
}