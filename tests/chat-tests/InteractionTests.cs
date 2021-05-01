using System;
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
        private readonly CancellationTokenSource testsLifetime;
        private readonly IChatServer chatServer;
        private readonly IChatClient chatClient;
        private readonly IChatTransport connectingClient;

        private Task serverTask;
        private Task clientTask;

        public InteractionTests()
        {
            testsLifetime = new CancellationTokenSource();
            this.testsLifetime = testsLifetime;
            var (connectedClient, clientTransport) = TestingTransport.CreatePair();
            this.connectingClient = connectedClient;
            this.chatClient = new ChatClientFactory(TestingLogger.CreateFactory())
                .CreateClient(clientTransport, new TestClientConfig("TestUser"));
            this.chatServer = new ChatServer(TestingLogger.CreateLogger<ChatServer>(),
                new ServerRoomsRegistry(TestingLogger.CreateLogger<ServerRoomsRegistry>()));
        }

        [SetUp]
        public void Setup()
        {
            serverTask = chatServer.RunAsync(testsLifetime.Token);
            clientTask = chatClient.RunAsync(testsLifetime.Token);
        }

        [TearDown]
        public async Task Teardown()
        {
            testsLifetime.Cancel();
            try
            {
                await Task.WhenAll(serverTask, clientTask);
            }
            catch (OperationCanceledException)
            {
            }
        }

        [Test, Timeout(1000)]
        public async Task ListRoomsUponConnectionTest()
        {
            await chatServer.AddClientAsync(connectingClient, testsLifetime.Token);

            var rooms = await chatClient.ListRooms(testsLifetime.Token);

            Assert.IsEmpty(rooms);
        }
        
        

        private class TestClientConfig : IChatClientConfiguration
        {
            public string UserName { get; }

            public TestClientConfig(string userName)
            {
                UserName = userName;
            }
        }
    }
}
