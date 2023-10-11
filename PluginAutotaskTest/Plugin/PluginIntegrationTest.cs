using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
using PluginAutotask.API.Read;
using PluginAutotask.API.Utility;
using PluginAutotask.DataContracts;
using PluginAutotask.Helper;
using Xunit;
using Record = Aunalytics.Sdk.Plugins.Record;

namespace PluginAutotaskTest.Plugin
{
    public class PluginIntegrationTest
    {
        private Settings GetSettings()
        {
            return new Settings()
            {
                // add to test
                ApiZone = @"",
                UserName = @"",
                Secret = @"",
                ApiIntegrationCode = @""
            };
    }

        private ConnectRequest GetConnectSettings()
        {
            var settings = GetSettings();
            
            return new ConnectRequest
            {
                SettingsJson = JsonConvert.SerializeObject(settings)
            };
        }

        private Schema GetTestSchema(string entityId = "BillingCodes")
        {
            return new Schema
            {
                Id = entityId,
                Name = entityId,
            };
        }

        private const string DefaultQuery = @"BillingCodes
{""Filter"":[{""field"":""Id"",""op"":""gte"",""value"":0}]}";
        private Schema GetUserDefinedSchema(string query = DefaultQuery)
        {
            return new Schema
            {
                Id = "custom",
                Name = "custom",
                Query = query,
            };
        }

        [Fact]
        public async Task ConnectSessionTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginAutotask.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();
            var disconnectRequest = new DisconnectRequest();

            // act
            var response = client.ConnectSession(request);
            var responseStream = response.ResponseStream;
            var records = new List<ConnectResponse>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
                client.Disconnect(disconnectRequest);
            }

            // assert
            Assert.Single(records);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ConnectTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginAutotask.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();

            // act
            var response = client.Connect(request);

            // assert
            Assert.IsType<ConnectResponse>(response);
            Assert.Equal("", response.SettingsError);
            Assert.Equal("", response.ConnectionError);
            Assert.Equal("", response.OauthError);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasAllTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginAutotask.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.All,
                SampleSize = 10
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Equal(68, response.Schemas.Count);

            var schema = response.Schemas[0];
            Assert.Equal("BillingCodes", schema.Id);
            Assert.Equal("BillingCodes", schema.Name);
            Assert.Equal("", schema.Query);
            Assert.Equal(10, schema.Sample.Count);
            Assert.Equal(15, schema.Properties.Count);
        
            var property = schema.Properties[0];
            Assert.Equal("afterHoursWorkType", property.Id);
            Assert.Equal("afterHoursWorkType", property.Name);
            Assert.False(property.IsKey);
            Assert.Equal("", property.Description);
            Assert.Equal(PropertyType.Integer, property.Type);
            Assert.Equal("integer", property.TypeAtSource);
            Assert.True(property.IsNullable);
            Assert.False(property.IsCreateCounter);
            Assert.False(property.IsUpdateCounter);
            
            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasRefreshTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginAutotask.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                SampleSize = 10,
                ToRefresh =
                {
                    GetTestSchema("BillingCodes")
                }
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Equal(1, response.Schemas.Count);
            
            var schema = response.Schemas[0];
            Assert.Equal("BillingCodes", schema.Id);
            Assert.Equal("BillingCodes", schema.Name);
            Assert.Equal("", schema.Query);
            Assert.Equal(10, schema.Sample.Count);
            Assert.Equal(15, schema.Properties.Count);
        
            var property = schema.Properties[0];
            Assert.Equal("afterHoursWorkType", property.Id);
            Assert.Equal("afterHoursWorkType", property.Name);
            Assert.False(property.IsKey);
            Assert.Equal("", property.Description);
            Assert.Equal(PropertyType.Integer, property.Type);
            Assert.Equal("integer", property.TypeAtSource);
            Assert.True(property.IsNullable);
            Assert.False(property.IsCreateCounter);
            Assert.False(property.IsUpdateCounter);
            

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasUserDefinedTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginAutotask.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var query = @"BillingCodes
{""Filter"":[{""field"":""Id"",""op"":""gte"",""value"":0}]}";

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                SampleSize = 10,
                ToRefresh =
                {
                    GetUserDefinedSchema(query)
                }
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Equal(1, response.Schemas.Count);
            
            var schema = response.Schemas[0];
            Assert.Equal("BillingCodes", schema.Id);
            Assert.Equal("BillingCodes", schema.Name);
            Assert.Equal("BillingCodes\n{\"Filter\":[{\"field\":\"Id\",\"op\":\"gte\",\"value\":0}]}", schema.Query);
            Assert.Equal(10, schema.Sample.Count);
            Assert.Equal(15, schema.Properties.Count);
        
            var property = schema.Properties[0];
            Assert.Equal("afterHoursWorkType", property.Id);
            Assert.Equal("afterHoursWorkType", property.Name);
            Assert.False(property.IsKey);
            Assert.Equal("", property.Description);
            Assert.Equal(PropertyType.Integer, property.Type);
            Assert.Equal("integer", property.TypeAtSource);
            Assert.True(property.IsNullable);
            Assert.False(property.IsCreateCounter);
            Assert.False(property.IsUpdateCounter);
            
            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginAutotask.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var schema = GetTestSchema("Invoices");

            var connectRequest = GetConnectSettings();

            var schemaRequest = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                ToRefresh = {schema}
            };

            var request = new ReadRequest()
            {
                DataVersions = new DataVersions
                {
                    JobId = "test"
                },
                JobId = "test",
            };

            // act
            client.Connect(connectRequest);
            var schemasResponse = client.DiscoverSchemas(schemaRequest);
            request.Schema = schemasResponse.Schemas[0];

            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Equal(70653, records.Count);

            var record = JsonConvert.DeserializeObject<Dictionary<string, object>>(records[0].DataJson);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }


        [Fact]
        public async Task ReadStreamLimitTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginAutotask.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var schema = GetTestSchema("Invoices");

            var connectRequest = GetConnectSettings();

            var schemaRequest = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                ToRefresh = {schema}
            };

            var request = new ReadRequest()
            {
                DataVersions = new DataVersions
                {
                    JobId = "test"
                },
                JobId = "test",
                Limit = 600
            };

            // act
            client.Connect(connectRequest);
            var schemasResponse = client.DiscoverSchemas(schemaRequest);
            request.Schema = schemasResponse.Schemas[0];

            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Equal(600, records.Count);

            var record = JsonConvert.DeserializeObject<Dictionary<string, object>>(records[0].DataJson);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamUserDefinedQueryTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginAutotask.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var query = @"Invoices
{""Filter"":[{""field"":""Id"",""op"":""gte"",""value"":0}]}";

            var schemaRequest = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                SampleSize = 0,
                ToRefresh =
                {
                    GetUserDefinedSchema(query)
                }
            };

            var request = new ReadRequest()
            {
                DataVersions = new DataVersions
                {
                    JobId = "test"
                },
                JobId = "test",
            };

            // act
            client.Connect(connectRequest);
            var schemasResponse = client.DiscoverSchemas(schemaRequest);
            request.Schema = schemasResponse.Schemas[0];

            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Equal(70653, records.Count);

            var record = JsonConvert.DeserializeObject<Dictionary<string, object>>(records[0].DataJson);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
    }
}