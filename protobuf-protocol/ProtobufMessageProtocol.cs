using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Critical.Chat.Protocol.Messages;
using Google.Protobuf;
using IMessage = Critical.Chat.Protocol.Messages.IMessage;

namespace Critical.Chat.Protocol.Protobuf
{
    public class ProtobufMessageProtocol : IMessageProtocol
    {
        private class ChatRoom : IChatRoom
        {
            public string Id { get; }
            public string Name { get; }

            public ChatRoom(string id, string name)
            {
                Id = id;
                Name = name;
            }

            internal static ChatRoom FromProtobuf(Protobuf.ChatRoom chatRoom)
            {
                return new ChatRoom(chatRoom.Id, chatRoom.Name);
            }
        }

        public async Task<IMessage> Parse(Stream sourceStream, CancellationToken token = default)
        {
            var messageBuffer = await sourceStream.ReadLengthPrefixedAsync();

            var message = ProtocolMessage.Parser.ParseFrom(messageBuffer);

            switch (message.MessageCase)
            {
                case ProtocolMessage.MessageOneofCase.None:
                    throw new Exception($"Unexpected None as message type [message={message}]");
                case ProtocolMessage.MessageOneofCase.ListRoomRequest:
                    return new Messages.ListRoomsRequest(message.Id);
                case ProtocolMessage.MessageOneofCase.ListRoomResponse:
                    var listRoomResponse = message.ListRoomResponse;
                    var rooms = listRoomResponse.Rooms.Select(ChatRoom.FromProtobuf).ToList();
                    return new Messages.ListRoomsResponse(message.Id, rooms);
                case ProtocolMessage.MessageOneofCase.HandshakeRequest:
                    return new Messages.HandshakeRequest(message.Id, message.HandshakeRequest.UserId);
                case ProtocolMessage.MessageOneofCase.HandshakeResponse:
                    return new Messages.HandshakeResponse(message.Id, message.HandshakeResponse.UserName);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task Serialize(IMessage message, Stream targetStream, CancellationToken token = default)
        {
            switch (message.Type)
            {
                case MessageType.ListRoomsRequest:
                {
                    var protoMessage = new ProtocolMessage {Id = message.Id, ListRoomRequest = new ListRoomsRequest()};
                    await WriteProtocolMessage(targetStream, protoMessage, token);
                    break;
                }
                case MessageType.ListRoomsResponse:
                {
                    var listRoomsResponse = CastMessage<Messages.ListRoomsResponse>(message);
                    var protoMessage = new ProtocolMessage()
                    {
                        Id = message.Id,
                        ListRoomResponse = new ListRoomsResponse()
                        {
                            Rooms =
                            {
                                listRoomsResponse.Rooms.Select(room => new Protobuf.ChatRoom()
                                {
                                    Id = room.Id,
                                    Name = room.Name
                                })
                            }
                        }
                    };
                    await WriteProtocolMessage(targetStream, protoMessage, token);
                    break;
                }
                case MessageType.CreateRoom:
                    break;
                case MessageType.JoinRoom:
                    break;
                case MessageType.LeaveRoom:
                    break;
                case MessageType.SendMessage:
                    break;
                case MessageType.ReceiveMessage:
                    break;
                case MessageType.HandshakeRequest:
                {
                    var handshakeRequest = CastMessage<Messages.HandshakeRequest>(message);
                    var protoMessage = new ProtocolMessage()
                    {
                        Id = message.Id,
                        HandshakeRequest = new HandshakeRequest()
                        {
                            UserId = handshakeRequest.UserId
                        }
                    };
                    await WriteProtocolMessage(targetStream, protoMessage, token);
                    break;
                }
                case MessageType.HandshakeResponse:
                {
                    var handshakeResponse = CastMessage<Messages.HandshakeResponse>(message);
                    var protoMessage = new ProtocolMessage()
                    {
                        Id = message.Id,
                        HandshakeResponse = new HandshakeResponse()
                        {
                            UserName = handshakeResponse.UserName
                        }
                    };
                    await WriteProtocolMessage(targetStream, protoMessage, token);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private TMessage CastMessage<TMessage>(IMessage message) where TMessage : IMessage
        {
            if (message is TMessage castMessage)
            {
                return castMessage;
            }

            throw new Exception($"Invalid message cast [message={message}][expected={typeof(TMessage)}]");
        }

        private static Task WriteProtocolMessage(Stream targetStream,
                                                 Google.Protobuf.IMessage message,
                                                 CancellationToken token = default)
        {
            var buffer = message.ToByteArray();
            return targetStream.WriteLengthPrefixedAsync(buffer, token);
        }
    }
}
