using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Client;
using Critical.Chat.Protocol.Transport;
using Critical.Chat.Server;
using Critical.Chat.Server.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Critical.Chat.Testing
{
    [TestClass]
    public class InteractionTests
    {
        private readonly IChatClientFactory clientFactory = new ChatClientFactory(TestingLogger.CreateFactory());
        private readonly IList<Task> pendingTasks = new List<Task>();
        private CancellationTokenSource testsLifetime;

        [TestInitialize]
        public void Setup()
        {
            testsLifetime = new CancellationTokenSource();
        }

        [TestCleanup]
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

        [TestMethod, Timeout(10000)]
        public async Task ListRoomsUponConnectionTest()
        {
            var chatServer = CreateServer();
            var (chatClient, connectingClient) = CreateClient();
            await chatServer.AddClientAsync(new ChatUser(Guid.NewGuid().ToString(), "TestUser"),
                                            connectingClient,
                                            testsLifetime.Token);

            var rooms = await chatClient.ListRooms(testsLifetime.Token);

            Assert.AreEqual(rooms.Count, 0);
        }

        [TestMethod, Timeout(1000)]
        public async Task ConnectTwiceWithSameClientTest()
        {
            var chatServer = CreateServer();
            var (_, connectingClient) = CreateClient();
            var chatUser = new ChatUser(Guid.NewGuid().ToString(), "TestUser");
            await chatServer.AddClientAsync(chatUser, connectingClient, testsLifetime.Token);

            try
            {
                Debug.WriteLine("Adding second client.");
                await chatServer.AddClientAsync(chatUser, connectingClient, testsLifetime.Token);
                Debug.WriteLine("Was not expecting this to succeed.");
                Assert.Fail();
            }
            catch (Exception error)
            {
                Debug.WriteLine($"{error}");
                // expected
            }
        }

        [TestMethod, Timeout(1000)]
        public async Task CreateAndConnectToRoom()
        {
            var chatServer = CreateServer();
            var (chatClient, connectingClient) = CreateClient();
            var chatUser = new ChatUser(Guid.NewGuid().ToString(), "TestUser");

            await chatServer.AddClientAsync(chatUser, connectingClient, testsLifetime.Token);

            var room = await chatClient.CreateRoom("testRoom", testsLifetime.Token);

            Assert.IsNotNull(room);

            var clientRoom = await chatClient.JoinRoom(room, testsLifetime.Token);

            Assert.IsNotNull(clientRoom);

            var users = await clientRoom.ListUsers(testsLifetime.Token);

            Assert.IsNotNull(users.FirstOrDefault(user => user.Id.Equals(chatUser.Id)));
        }

        [TestMethod, Timeout(1000)]
        public async Task TwoClientsJoiningSameRoom()
        {
            var chatServer = CreateServer();
            var (firstClient, firstConnection) = CreateClient();
            var firstUser = new ChatUser(Guid.NewGuid().ToString(), "TestUserA");
            var (secondClient, secondConnection) = CreateClient();
            var secondUser = new ChatUser(Guid.NewGuid().ToString(), "TestUserB");

            await Task.WhenAll(chatServer.AddClientAsync(firstUser, firstConnection, testsLifetime.Token),
                               chatServer.AddClientAsync(secondUser, secondConnection, testsLifetime.Token));

            var chatRoom = await firstClient.CreateRoom("testRoom", testsLifetime.Token);

            var connections = await Task.WhenAll(firstClient.JoinRoom(chatRoom, testsLifetime.Token),
                                                 secondClient.JoinRoom(chatRoom, testsLifetime.Token));
            Assert.AreEqual(connections.Length, 2);

            var firstRoom = connections[0];
            var secondRoom = connections[1];

            Assert.AreEqual(firstRoom.Id, secondRoom.Id);

            var firstMessage = $"Hello from {firstUser.Name}";
            var secondMessage = $"Hello from {secondUser.Name}";

            await firstRoom.SendMessage(firstMessage, testsLifetime.Token);

            var firstIncoming = await secondRoom.GetMessage(testsLifetime.Token);

            Assert.AreEqual(firstMessage, firstIncoming.Body);
            Assert.AreEqual(firstUser.Id, firstIncoming.Sender.Id);
            Assert.AreEqual(chatRoom.Id, firstIncoming.Room.Id);
            
            await secondRoom.SendMessage(secondMessage, testsLifetime.Token);

            // Note that we check `secondRoom`'s messages again to see if messages are echoed (and because
            // first room still have first message unhandled).
            var secondIncoming = await secondRoom.GetMessage(testsLifetime.Token);
            
            Assert.AreEqual(secondMessage, secondIncoming.Body);
            Assert.AreEqual(secondUser.Id, secondIncoming.Sender.Id);
            Assert.AreEqual(chatRoom.Id, secondIncoming.Room.Id);
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
            var roomRegistry = new RoomRegistry(TestingLogger.CreateLogger<RoomRegistry>());
            var chatServer = new ChatServer(TestingLogger.CreateLogger<ChatServer>(), roomRegistry);

            pendingTasks.Add(chatServer.RunAsync(testsLifetime.Token).ContinueWith(_ => roomRegistry.Close()));
            return chatServer;
        }
    }
}
