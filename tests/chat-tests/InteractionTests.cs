using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Client;
using Lemvik.Example.Chat.Client.Implementation;
using Lemvik.Example.Chat.Protocol;
using Lemvik.Example.Chat.Protocol.Transport;
using Lemvik.Example.Chat.Server;
using Lemvik.Example.Chat.Server.Implementation;
using Lemvik.Example.Chat.Shared;
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

        [TestMethod, Timeout(1000)]
        public async Task ListRoomsUponConnectionTest()
        {
            var existingRooms = new[]
            {
                new ChatRoom(Guid.NewGuid().ToString(), "RoomA"),
                new ChatRoom(Guid.NewGuid().ToString(), "RoomB")
            };
            var chatServer = await CreateServer(existingRooms);
            var (chatClient, connectingClient) = CreateClient();
            var clientTask = await chatServer.AddClientAsync(new ChatUser(Guid.NewGuid().ToString(), "TestUser"),
                                                             connectingClient,
                                                             testsLifetime.Token);
            pendingTasks.Add(clientTask);

            var rooms = await chatClient.ListRooms(testsLifetime.Token);

            Assert.AreEqual(existingRooms.Length, rooms.Count);
        }


        [TestMethod, Timeout(1000)]
        public async Task CancellationStopsClients()
        {
            var clientControl = new CancellationTokenSource();
            var existingRoom = new ChatRoom(Guid.NewGuid().ToString(), "TestRoom");
            var chatServer = await CreateServer(new[] {existingRoom});

            var (client, connectingClient) = CreateClient(clientControl.Token);
            var chatUser = new ChatUser(Guid.NewGuid().ToString(), "TestUser");
            var clientTask = await chatServer.AddClientAsync(chatUser, connectingClient, testsLifetime.Token);
            pendingTasks.Add(clientTask);

            var rooms = await client.ListRooms(testsLifetime.Token);

            Assert.AreEqual(1, rooms.Count);

            var (observerClient, observerConnection) = CreateClient();
            var observerUser = new ChatUser(Guid.NewGuid().ToString(), "Observer");
            clientTask = await chatServer.AddClientAsync(observerUser, observerConnection, testsLifetime.Token);
            pendingTasks.Add(clientTask);

            var observerRoom = await observerClient.JoinRoom(existingRoom, testsLifetime.Token);

            var clientRoom = await client.JoinRoom(existingRoom, testsLifetime.Token);

            var users = await clientRoom.ListUsers(testsLifetime.Token);

            Assert.AreEqual(2, users.Count);

            clientControl.Cancel();

            try
            {
                await client.ListRooms(testsLifetime.Token);
                Assert.Fail("After client has been stopped we should not be able to use it.");
            }
            catch (OperationCanceledException)
            {
            }

            try
            {
                await clientRoom.ListUsers(testsLifetime.Token);
                Assert.Fail("After client has been stopped related rooms should not be functional.");
            }
            catch (OperationCanceledException)
            {
            }

            // This is needed here as terminating the client on server side is an operation that:
            // 1. takes some time
            // 2. not awaited anywhere in these tests
            await Task.Delay(100, testsLifetime.Token);
            var postCancelUsers = await observerRoom.ListUsers(testsLifetime.Token);

            Assert.AreEqual(1, postCancelUsers.Count);
        }

        [TestMethod, Timeout(1000)]
        public async Task ConnectTwiceWithSameClientTest()
        {
            var chatServer = await CreateServer(Array.Empty<ChatRoom>());
            var (_, connectingClient) = CreateClient();
            var chatUser = new ChatUser(Guid.NewGuid().ToString(), "TestUser");
            var clientTask = await chatServer.AddClientAsync(chatUser, connectingClient, testsLifetime.Token);
            pendingTasks.Add(clientTask);

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

            var clientTask = await chatServer.AddClientAsync(chatUser, connectingClient, testsLifetime.Token);
            pendingTasks.Add(clientTask);

            var room = await chatClient.CreateRoom("testRoom", testsLifetime.Token);

            Assert.IsNotNull(room);

            var clientRoom = await chatClient.JoinRoom(room, testsLifetime.Token);

            Assert.IsNotNull(clientRoom);

            var users = await clientRoom.ListUsers(testsLifetime.Token);

            Assert.IsNotNull(users.FirstOrDefault(user => user.Id.Equals(chatUser.Id)));
        }

        [TestMethod, Timeout(1000)]
        public async Task FailToEnterRoomTwice()
        {
            var existingRoom = new ChatRoom(Guid.NewGuid().ToString(), "TestRoom");
            var chatServer = await CreateServer(new[] {existingRoom});
            var (chatClient, connectingClient) = CreateClient();
            var chatUser = new ChatUser(Guid.NewGuid().ToString(), "TestUser");

            var clientTask = await chatServer.AddClientAsync(chatUser, connectingClient, testsLifetime.Token);
            pendingTasks.Add(clientTask);

            var clientRoom = await chatClient.JoinRoom(existingRoom, testsLifetime.Token);

            var users = await clientRoom.ListUsers(testsLifetime.Token);

            Assert.AreEqual(1, users.Count);

            try
            {
                await chatClient.JoinRoom(existingRoom, testsLifetime.Token);
            }
            catch (ChatException)
            {
                // expected
            }

            users = await clientRoom.ListUsers(testsLifetime.Token);

            Assert.AreEqual(1, users.Count);
        }

        [TestMethod, Timeout(1000)]
        public async Task EnterLeaveAndEnterRoom()
        {
            var existingRoom = new ChatRoom(Guid.NewGuid().ToString(), "TestRoom");
            var chatServer = await CreateServer(new[] {existingRoom});
            var (chatClient, connectingClient) = CreateClient();
            var chatUser = new ChatUser(Guid.NewGuid().ToString(), "TestUser");

            var clientTask = await chatServer.AddClientAsync(chatUser, connectingClient, testsLifetime.Token);
            pendingTasks.Add(clientTask);

            var (observerClient, observerConnection) = CreateClient();
            var observerUser = new ChatUser(Guid.NewGuid().ToString(), "Observer");
            clientTask = await chatServer.AddClientAsync(observerUser, observerConnection, testsLifetime.Token);
            pendingTasks.Add(clientTask);

            var observerRoom = await observerClient.JoinRoom(existingRoom, testsLifetime.Token);

            var clientRoom = await chatClient.JoinRoom(existingRoom, testsLifetime.Token);

            var firstUsers = await clientRoom.ListUsers(testsLifetime.Token);

            Assert.AreEqual(2, firstUsers.Count);

            await clientRoom.Leave(testsLifetime.Token);

            var observerUsers = await observerRoom.ListUsers(testsLifetime.Token);

            Assert.AreEqual(1, observerUsers.Count);

            try
            {
                await clientRoom.SendMessage("Should not be delivered", testsLifetime.Token);
                Assert.Fail("Sending message after leaving room should fail.");
            }
            catch (OperationCanceledException)
            {
            }

            var anotherRoomInstance = await chatClient.JoinRoom(existingRoom, testsLifetime.Token);

            var secondUsers = await anotherRoomInstance.ListUsers(testsLifetime.Token);

            Assert.AreEqual(2, secondUsers.Count);
        }

        [TestMethod, Timeout(1000)]
        public async Task TwoClientsJoiningSameRoom()
        {
            var chatRoom = new ChatRoom(Guid.NewGuid().ToString(), "TestRoom");
            var chatServer = await CreateServer(new[] {chatRoom});
            var (firstClient, firstConnection) = CreateClient();
            var firstUser = new ChatUser(Guid.NewGuid().ToString(), "TestUserA");
            var (secondClient, secondConnection) = CreateClient();
            var secondUser = new ChatUser(Guid.NewGuid().ToString(), "TestUserB");

            pendingTasks.Add(await chatServer.AddClientAsync(firstUser, firstConnection, testsLifetime.Token));
            pendingTasks.Add(await chatServer.AddClientAsync(secondUser, secondConnection, testsLifetime.Token));

            var connections = await Task.WhenAll(firstClient.JoinRoom(chatRoom, testsLifetime.Token),
                                                 secondClient.JoinRoom(chatRoom, testsLifetime.Token));
            Assert.AreEqual(connections.Length, 2);

            var firstRoom = connections[0];
            var secondRoom = connections[1];

            Assert.AreEqual(firstRoom.ChatRoom.Id, secondRoom.ChatRoom.Id);

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

            var clientTask = await chatServer.AddClientAsync(firstUser, firstConnection, testsLifetime.Token);
            pendingTasks.Add(clientTask);
            var chatRoom = await firstClient.CreateRoom(roomName, testsLifetime.Token);
            var firstRoom = await firstClient.JoinRoom(chatRoom, testsLifetime.Token);

            var messages = new[] {"Spam!", "Spam, spam, spam!", "More spam!"};

            foreach (var message in messages)
            {
                await firstRoom.SendMessage(message, testsLifetime.Token);
            }

            var (secondClient, secondConnection) = CreateClient();
            var secondUser = new ChatUser(Guid.NewGuid().ToString(), "TestUserA");

            clientTask = await chatServer.AddClientAsync(secondUser, secondConnection, testsLifetime.Token);
            pendingTasks.Add(clientTask);
            var secondRoom = await secondClient.JoinRoom(chatRoom, testsLifetime.Token);

            foreach (var message in messages)
            {
                var receivedMessage = await secondRoom.GetMessage(testsLifetime.Token);
                Assert.AreEqual(message, receivedMessage.Body);
            }
        }

        private (IChatClient, IChatTransport) CreateClient(CancellationToken token = default)
        {
            var clientToken = CancellationTokenSource.CreateLinkedTokenSource(testsLifetime.Token, token).Token;
            var (connectedClient, clientTransport) = TestingTransport.CreatePair();
            var chatClient = clientFactory.CreateClient(clientTransport);
            pendingTasks.Add(chatClient.RunAsync(clientToken));
            return (chatClient, connectedClient);
        }

        private Task<IChatServer> CreateServer(ICollection<ChatRoom> initialRooms)
        {
            var trackingFactory = InMemoryMessageTracker.Factory;
            var roomsSource =
                new InMemoryRoomSourceFactory(trackingFactory, new InMemoryRoomBackplaneFactory(), initialRooms);
            var roomRegistry = new RoomRegistry(roomsSource);
            var chatServer = new ChatServer(TestingLogger.CreateFactory(), roomRegistry);

            pendingTasks.Add(chatServer.RunAsync(testsLifetime.Token));
            pendingTasks.Add(roomRegistry.RunAsync(testsLifetime.Token));
            return Task.FromResult<IChatServer>(chatServer);
        }
    }
}
