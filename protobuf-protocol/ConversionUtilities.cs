using System;
using System.Linq;
using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Protocol.Protobuf
{
    internal static class ConversionUtilities
    {
        internal static TMessage CastMessage<TMessage>(this IMessage message) where TMessage : IMessage
        {
            if (message is TMessage castMessage)
            {
                return castMessage;
            }

            throw new Exception($"Invalid message cast [message={message}][expected={typeof(TMessage)}]");
        }

        internal static ProtocolMessage ToProtocolMessage(this Messages.ListRoomsRequest listRoomsRequest)
        {
            return new ProtocolMessage {Id = listRoomsRequest.Id, ListRoomRequest = new ListRoomsRequest()};
        }

        internal static ProtocolMessage ToProtocolMessage(this Messages.ListRoomsResponse listRoomsResponse)
        {
            return new ProtocolMessage()
            {
                Id = listRoomsResponse.Id,
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
        }

        internal static ProtocolMessage ToProtocolMessage(this Messages.HandshakeRequest handshakeRequest)
        {
            return new ProtocolMessage()
            {
                Id = handshakeRequest.Id,
                HandshakeRequest = new HandshakeRequest()
                {
                    UserId = handshakeRequest.UserId
                }
            };
        }

        internal static ProtocolMessage ToProtocolMessage(this Messages.HandshakeResponse handshakeResponse)
        {
            return new ProtocolMessage()
            {
                Id = handshakeResponse.Id,
                HandshakeResponse = new HandshakeResponse()
                {
                    UserName = handshakeResponse.UserName
                }
            };
        }

        internal static ProtocolMessage ToProtocolMessage(this Messages.IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.ListRoomsRequest:
                    return message.CastMessage<Messages.ListRoomsRequest>().ToProtocolMessage();
                case MessageType.ListRoomsResponse:
                    return message.CastMessage<Messages.ListRoomsResponse>().ToProtocolMessage();
                case MessageType.HandshakeRequest:
                    return message.CastMessage<Messages.HandshakeRequest>().ToProtocolMessage();
                case MessageType.HandshakeResponse:
                    return message.CastMessage<Messages.HandshakeResponse>().ToProtocolMessage();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

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

        internal static IMessage FromProtocolMessage(this ProtocolMessage message)
        {
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
    }
}
