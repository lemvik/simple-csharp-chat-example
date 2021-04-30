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
                    var protoMessage = new ProtocolMessage();
                    protoMessage.Id = message.Id;
                    protoMessage.ListRoomRequest = new ListRoomsRequest();
                    var buffer = protoMessage.ToByteArray();
                    await targetStream.WriteLengthPrefixedAsync(buffer, token);
                    break;
                }
                case MessageType.ListRoomsResponse:
                {
                    if (message is Messages.ListRoomsResponse listRoomsResponse)
                    {
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
                        var buffer = protoMessage.ToByteArray();
                        await targetStream.WriteLengthPrefixedAsync(buffer, token);
                    }
                    else
                    {
                        throw new Exception($"Message type mismatch [message={message}]");
                    }

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
