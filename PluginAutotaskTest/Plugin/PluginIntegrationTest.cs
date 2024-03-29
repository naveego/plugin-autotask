using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Aunalytics.Sdk.Plugins;
using Newtonsoft.Json;
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
                ApiIntegrationCode = @"",
                ApiUsageThreshold = 5000,
                ApiDelayIntervalSeconds = 300,
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
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
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
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
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
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
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
            Assert.Equal(74, response.Schemas.Count);

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
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
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
        public async Task DiscoverRangedTicketHistorySchemasTest()
        {
            // setup
            Server server = new Server
            {
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
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
                    GetTestSchema("TicketHistoryLast05Days"),
                    GetTestSchema("TicketHistoryLast30Days")
                }
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Equal(2, response.Schemas.Count);

            var schemaLast5Days = response.Schemas[0];
            Assert.Equal("TicketHistoryLast05Days", schemaLast5Days.Id);
            Assert.Equal("TicketHistoryLast05Days", schemaLast5Days.Name);
            Assert.Equal("", schemaLast5Days.Query);
            Assert.Equal(10, schemaLast5Days.Sample.Count);
            Assert.Equal(6, schemaLast5Days.Properties.Count);

            var pkLast5Days = schemaLast5Days.Properties[3];
            Assert.Equal("id", pkLast5Days.Id);
            Assert.Equal("id", pkLast5Days.Name);
            Assert.True(pkLast5Days.IsKey);
            Assert.Equal("", pkLast5Days.Description);
            Assert.Equal(PropertyType.Integer, pkLast5Days.Type);
            Assert.Equal("long", pkLast5Days.TypeAtSource);
            Assert.False(pkLast5Days.IsNullable);
            Assert.False(pkLast5Days.IsCreateCounter);
            Assert.False(pkLast5Days.IsUpdateCounter);

            var schemaLast30Days = response.Schemas[1];
            Assert.Equal("TicketHistoryLast30Days", schemaLast30Days.Id);
            Assert.Equal("TicketHistoryLast30Days", schemaLast30Days.Name);
            Assert.Equal("", schemaLast30Days.Query);
            Assert.Equal(10, schemaLast30Days.Sample.Count);
            Assert.Equal(6, schemaLast30Days.Properties.Count);

            var pkLast30Days = schemaLast30Days.Properties[3];
            Assert.Equal("id", pkLast30Days.Id);
            Assert.Equal("id", pkLast30Days.Name);
            Assert.True(pkLast30Days.IsKey);
            Assert.Equal("", pkLast30Days.Description);
            Assert.Equal(PropertyType.Integer, pkLast30Days.Type);
            Assert.Equal("long", pkLast30Days.TypeAtSource);
            Assert.False(pkLast30Days.IsNullable);
            Assert.False(pkLast30Days.IsCreateCounter);
            Assert.False(pkLast30Days.IsUpdateCounter);

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
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
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
        public async Task DiscoverSchemasDynamicDateTest()
        {
            // setup
            Server server = new Server
            {
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var query = @"Invoices
{""Filter"":[{""field"":""invoiceDateTime"",""op"":""gte"",""value"":""TODAYMINUS_7_DAYS""}]}";

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
            Assert.Equal("custom", schema.Id);
            Assert.Equal("custom", schema.Name);
            Assert.Equal("Invoices\r\n{\"Filter\":[{\"field\":\"invoiceDateTime\",\"op\":\"gte\",\"value\":\"TODAYMINUS_7_DAYS\"}]}", schema.Query);
            Assert.Equal(10, schema.Sample.Count);
            Assert.Equal(23, schema.Properties.Count);

            var property = schema.Properties[0];
            Assert.Equal("batchID", property.Id);
            Assert.Equal("batchID", property.Name);
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
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
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
                ToRefresh = { schema }
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
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
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
                ToRefresh = { schema }
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

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamTicketHistoryLimitTest()
        {
            // setup
            Server server = new Server
            {
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var schema = GetTestSchema("TicketHistoryLast05Days");

            var connectRequest = GetConnectSettings();

            var schemaRequest = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                ToRefresh = { schema }
            };

            var request = new ReadRequest()
            {
                DataVersions = new DataVersions
                {
                    JobId = "test"
                },
                JobId = "test",
                Limit = 50
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
            Assert.Equal(50, records.Count);

            var todayMinus5Days = DateTime.Today.AddDays(-5);
            Assert.True(records.All(r => {
                var recordDate = JsonConvert.DeserializeObject<Dictionary<string, object>>(r.DataJson);
                return (DateTime)recordDate["date"] >= todayMinus5Days;
            }));

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
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
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

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamDynamicDateTest()
        {
            // setup
            Server server = new Server
            {
                Services = { Publisher.BindService(new PluginAutotask.Plugin.Plugin()) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var query = @"Invoices
{""Filter"":[{""field"":""invoiceDateTime"",""op"":""gte"",""value"":""TODAYMINUS_7_DAYS""}]}";

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
            Assert.Equal(719, records.Count);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
    }
}