using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Client;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Transport;
using Lemvik.Example.Chat.Server;
using Lemvik.Example.Chat.Server.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lemvik.Example.Chat.Testing
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
            var existingRooms = new[]
            {
                new ChatRoom(Guid.NewGuid().ToString(), "RoomA"),
                new ChatRoom(Guid.NewGuid().ToString(), "RoomB")
            };
            var chatServer = await CreateServer(existingRooms);
            var (chatClient, connectingClient) = CreateClient();
            await chatServer.AddClientAsync(new ChatUser(Guid.NewGuid().ToString(), "TestUser"),
                                            connectingClient,
                                            testsLifetime.Token);

            var rooms = await chatClient.ListRooms(testsLifetime.Token);

            Assert.AreEqual(existingRooms.Length, rooms.Count);
        }

        [TestMethod, Timeout(1000)]
        public async Task ConnectTwiceWithSameClientTest()
        {
            var chatServer = await CreateServer(Array.Empty<ChatRoom>());
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
            catch (ChatException)
            {
                // expected
            }
        }

        [TestMethod, Timeout(1000)]
        public async Task CreateAndConnectToRoom()
        {
            var chatServer = await CreateServer(Array.Empty<ChatRoom>());
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
            var chatServer = await CreateServer(Array.Empty<ChatRoom>());
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

            Assert.AreEqual(firstRoom.Room.Id, secondRoom.Room.Id);

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

        [TestMethod, Timeout(1000)]
        public async Task CheckExistingMessages()
        {
            var chatServer = await CreateServer(Array.Empty<ChatRoom>());
            const string roomName = "testRoom";
            var (firstClient, firstConnection) = CreateClient();
            var firstUser = new ChatUser(Guid.NewGuid().ToString(), "TestUserA");

            await chatServer.AddClientAsync(firstUser, firstConnection, testsLifetime.Token);
            var chatRoom = await firstClient.CreateRoom(roomName, testsLifetime.Token);
            var firstRoom = await firstClient.JoinRoom(chatRoom, testsLifetime.Token);

            var messages = new[] {"Spam!", "Spam, spam, spam!", "More spam!"};

            await Task.WhenAll(messages.Select(message => firstRoom.SendMessage(message, testsLifetime.Token)));
            
            var (secondClient, secondConnection) = CreateClient();
            var secondUser = new ChatUser(Guid.NewGuid().ToString(), "TestUserA");
            
            await chatServer.AddClientAsync(secondUser, secondConnection, testsLifetime.Token);
            var secondRoom = await secondClient.JoinRoom(chatRoom, testsLifetime.Token);

            foreach (var message in messages)
            {
                var receivedMessage = await secondRoom.GetMessage(testsLifetime.Token);
                Assert.AreEqual(message, receivedMessage.Body);
            }
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

        private async Task<IChatServer> CreateServer(IReadOnlyCollection<ChatRoom> initialRooms)
        {
            var trackingFactory = InMemoryMessageTracker.Factory;
            var roomsSource = new TransientRoomSource(trackingFactory);
            await roomsSource.Initialize(initialRooms);
            var roomRegistry = new RoomRegistry(TestingLogger.CreateLogger<RoomRegistry>(), roomsSource);
            var chatServer = new ChatServer(TestingLogger.CreateLogger<ChatServer>(), roomRegistry);

            pendingTasks.Add(chatServer.RunAsync(testsLifetime.Token));
            pendingTasks.Add(roomRegistry.RunAsync(testsLifetime.Token));
            return chatServer;
        }
    }
}
