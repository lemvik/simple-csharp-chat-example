using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Client;
using Critical.Chat.Protocol.Transport;
using Critical.Chat.Server;
using Critical.Chat.Server.Implementation;
using NUnit.Framework;

namespace Critical.Chat.Testing
{
    [TestFixture]
    public class InteractionTests
    {
        private readonly IChatClientFactory clientFactory = new ChatClientFactory(TestingLogger.CreateFactory());
        private readonly IList<Task> pendingTasks = new List<Task>();
        private CancellationTokenSource testsLifetime;

        [SetUp]
        public void Setup()
        {
            testsLifetime = new CancellationTokenSource();
        }

        [TearDown]
        public async Task Teardown()
        {
            testsLifetime.Cancel();
            try
            {
                await Task.WhenAll(pendingTasks);
            }
            catch (OperationCanceledException)
            {
            }
            
            pendingTasks.Clear();
        }

        [Test, Timeout(1000)]
        public async Task ListRoomsUponConnectionTest()
        {
            var chatServer = CreateServer();
            var (chatClient, connectingClient) = CreateClient();
            await chatServer.AddClientAsync(new ChatUser(Guid.NewGuid().ToString(), "TestUser"),
                                            connectingClient,
                                            testsLifetime.Token);

            var rooms = await chatClient.ListRooms(testsLifetime.Token);

            Assert.IsEmpty(rooms);
        }

        [Test, Timeout(1000)]
        public async Task ConnectTwiceWithSameClientTest()
        {
            var chatServer = CreateServer();
            var (_, connectingClient) = CreateClient();
            var chatUser = new ChatUser(Guid.NewGuid().ToString(), "TestUser");
            await chatServer.AddClientAsync(chatUser, connectingClient, testsLifetime.Token);

            Assert.ThrowsAsync<Exception>(async () => await chatServer.AddClientAsync(chatUser,
                                              connectingClient,
                                              testsLifetime.Token));
        }

        private class TestClientConfig : IChatClientConfiguration
        {
            public string UserName { get; }

            public TestClientConfig(string userName)
            {
                UserName = userName;
            }
        }

        private (IChatClient, IChatTransport) CreateClient()
        {
            var (connectedClient, clientTransport) = TestingTransport.CreatePair();
            var chatClient = clientFactory.CreateClient(clientTransport, new TestClientConfig("TestUser"));
            pendingTasks.Add(chatClient.RunAsync(testsLifetime.Token));
            return (chatClient, connectedClient);
        }

        private IChatServer CreateServer()
        {
            var chatServer = new ChatServer(TestingLogger.CreateLogger<ChatServer>(),
                                        new ServerRoomsRegistry(TestingLogger
                                                                    .CreateLogger<ServerRoomsRegistry>()));

            pendingTasks.Add(chatServer.RunAsync(testsLifetime.Token));
            return chatServer;
        }
    }
}
