﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol;
using Critical.Chat.Protocol.Messages;
using Critical.Chat.Protocol.Transport;
using Microsoft.Extensions.Logging;

namespace Critical.Chat.Client
{
    internal class ChatClient : IChatClient
    {
        private readonly ILogger<ChatClient> logger;
        private readonly IChatTransport transport;
        private readonly IChatClientConfiguration configuration;
        private readonly IDictionary<ulong, TaskCompletionSource<IMessage>> pendingMessages;
        private ulong sequence;
        private string assignedId = string.Empty;

        internal ChatClient(ILogger<ChatClient> logger, 
                            IChatTransport transport,
                            IChatClientConfiguration configuration)
        {
            this.logger = logger;
            this.transport = transport;
            this.configuration = configuration;
            this.pendingMessages = new Dictionary<ulong, TaskCompletionSource<IMessage>>();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            await HandshakeAsync(token);
            
            logger.LogDebug("Handshake successful [clientId={clientId}]", assignedId);
            
            while (!token.IsCancellationRequested)
            {
                var incomingMessage = await transport.Receive(token);

                if (pendingMessages.TryGetValue(incomingMessage.Id, out var pendingRequest))
                {
                    pendingMessages.Remove(incomingMessage.Id);
                    pendingRequest.SetResult(incomingMessage);
                }
                else
                {
                    await DispatchMessage(incomingMessage, token);
                }
            }
        }

        public async Task<IReadOnlyCollection<IChatRoom>> ListRooms(CancellationToken token = default)
        {
            var request = new ListRoomsRequest(++sequence);

            var response = await Exchange<ListRoomsResponse>(request, token);

            return response.Rooms;
        }

        public async Task<IChatRoom> CreateRoom(string roomName, CancellationToken token = default)
        {
            var request = new CreateRoomRequest(++sequence, roomName);

            var response = await Exchange<CreateRoomResponse>(request, token);

            return response.Room;
        }

        public Task<IChatRoomUser> JoinRoom(IChatRoom room, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        private async Task HandshakeAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var incomingMessage = await transport.Receive(token);
                if (incomingMessage is HandshakeRequest handshakeRequest)
                {
                    assignedId = handshakeRequest.UserId;
                    var response = new HandshakeResponse(handshakeRequest.Id, configuration.UserName);
                    await transport.Send(response, token);
                    return;
                }

                logger.LogWarning("Ignoring [message={message}] while waiting for handshake.", incomingMessage);
            }
        }

        private async Task<TResult> Exchange<TResult>(IMessage message, CancellationToken token = default)
            where TResult : class, IMessage
        {
            var completion = new TaskCompletionSource<IMessage>();
            pendingMessages.Add(message.Id, completion);

            await transport.Send(message, token);

            var response = await completion.Task;

            if (response is TResult result)
            {
                return result;
            }

            throw new Exception($"Received unexpected [response={response}] for [request={message}]");
        }

        private Task DispatchMessage(IMessage message, CancellationToken token = default)
        {
            logger.LogDebug("Dispatching [message={message}]", message);

            switch (message.Type)
            {
                case MessageType.JoinRoom:
                    break;
                case MessageType.LeaveRoom:
                    break;
                case MessageType.SendMessage:
                    break;
                case MessageType.ReceiveMessage:
                    break;
                default:
                    logger.LogWarning("Unexpected message to be handled [message={message}]", message);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
