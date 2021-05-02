using System;
using System.Linq;
using Critical.Chat.Protocol.Messages;

namespace Critical.Chat.Protocol.Protobuf
{
    internal static class ConversionUtilities
    {
        private static Protobuf.ChatRoom ToProtobuf(this IChatRoom chatRoom)
        {
            return new Protobuf.ChatRoom()
            {
                Id = chatRoom.Id,
                Name = chatRoom.Name
            };
        }

        private static Protobuf.ChatUser ToProtobuf(this IChatUser chatUser)
        {
            return new Protobuf.ChatUser()
            {
                UserId = chatUser.Id,
                UserName = chatUser.Name
            };
        }

        private static UserMessage ToProtobuf(this IChatMessage chatMessage)
        {
            return new UserMessage()
            {
                User = chatMessage.Sender.ToProtobuf(),
                Room = chatMessage.Room.ToProtobuf(),
                Message = chatMessage.Body
            };
        }

        private static ProtocolExchange ToProtocolExchange(this ExchangeMessage message)
        {
            switch (message.Message)
            {
                case Messages.ListRoomsRequest _:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        ListRoomRequest = new ListRoomsRequest()
                    };
                case Messages.ListRoomsResponse listRoomsResponse:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        ListRoomResponse = new ListRoomsResponse
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
                case Messages.HandshakeRequest handshakeRequest:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        HandshakeRequest = new HandshakeRequest
                        {
                            User = handshakeRequest.User.ToProtobuf()
                        }
                    };
                case Messages.HandshakeResponse _:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        HandshakeResponse = new HandshakeResponse()
                    };
                case Messages.CreateRoomRequest createRoomRequest:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        CreateRoomRequest = new CreateRoomRequest
                        {
                            RoomName = createRoomRequest.RoomName
                        }
                    };
                case Messages.CreateRoomResponse createRoomResponse:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        CreateRoomResponse = new CreateRoomResponse
                        {
                            Room = createRoomResponse.Room.ToProtobuf()
                        }
                    };
                case Messages.JoinRoomRequest joinRoomRequest:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        JoinRoomRequest = new JoinRoomRequest
                        {
                            RoomId = joinRoomRequest.RoomId
                        }
                    };
                case Messages.JoinRoomResponse joinRoomResponse:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        JoinRoomResponse = new JoinRoomResponse
                        {
                            Room = joinRoomResponse.Room.ToProtobuf(),
                            Messages =
                            {
                                joinRoomResponse.Messages.Select(chatMessage => chatMessage.ToProtobuf())
                            }
                        }
                    };
                case Messages.LeaveRoomRequest leaveRoomRequest:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        LeaveRoomRequest = new LeaveRoomRequest
                        {
                            Room = leaveRoomRequest.Room.ToProtobuf()
                        }
                    };
                case Messages.LeaveRoomResponse leaveRoomResponse:
                    return new ProtocolExchange
                    {
                        ExchangeId = message.ExchangeId,
                        LeaveRoomResponse = new LeaveRoomResponse
                        {
                            Room = leaveRoomResponse.Room.ToProtobuf()
                        }
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static ProtocolMessage ToProtocolMessage(this IMessage message)
        {
            switch (message)
            {
                case ExchangeMessage exchangeMessage:
                    return new ProtocolMessage {ExchangeMessage = exchangeMessage.ToProtocolExchange()};
                case ChatMessage chatMessage:
                    return new ProtocolMessage {UserMessage = chatMessage.ToProtobuf()};
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private class ChatUser : IChatUser
        {
            public string Id { get; }
            public string Name { get; }

            private ChatUser(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public static ChatUser FromProtobuf(Protobuf.ChatUser user)
            {
                return new ChatUser(user.UserId, user.UserName);
            }
        }

        private class ChatRoom : IChatRoom
        {
            public string Id { get; }
            public string Name { get; }

            private ChatRoom(string id, string name)
            {
                Id = id;
                Name = name;
            }

            internal static ChatRoom FromProtobuf(Protobuf.ChatRoom chatRoom)
            {
                return new ChatRoom(chatRoom.Id, chatRoom.Name);
            }
        }

        private static ChatMessage FromProtobuf(this UserMessage message)
        {
            return new ChatMessage(ChatUser.FromProtobuf(message.User),
                                   ChatRoom.FromProtobuf(message.Room),
                                   message.Message);
        }

        internal static IMessage FromProtocolMessage(this ProtocolMessage message)
        {
            switch (message.MessageCase)
            {
                case ProtocolMessage.MessageOneofCase.ExchangeMessage:
                    return message.ExchangeMessage.FromProtobuf();
                case ProtocolMessage.MessageOneofCase.UserMessage:
                    return message.UserMessage.FromProtobuf();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ExchangeMessage FromProtobuf(this ProtocolExchange protocolExchange)
        {
            return new ExchangeMessage(protocolExchange.ExchangeId, protocolExchange.PayloadFromProtobuf());
        }

        private static IMessage PayloadFromProtobuf(this ProtocolExchange protocolExchange)
        {
            switch (protocolExchange.MessageCase)
            {
                case ProtocolExchange.MessageOneofCase.HandshakeRequest:
                    return new Messages.HandshakeRequest(ChatUser.FromProtobuf(protocolExchange.HandshakeRequest.User));
                case ProtocolExchange.MessageOneofCase.HandshakeResponse:
                    return new Messages.HandshakeResponse();
                case ProtocolExchange.MessageOneofCase.CreateRoomRequest:
                    return new Messages.CreateRoomRequest(protocolExchange.CreateRoomRequest.RoomName);
                case ProtocolExchange.MessageOneofCase.CreateRoomResponse:
                    return new Messages.CreateRoomResponse(ChatRoom.FromProtobuf(protocolExchange.CreateRoomResponse
                                                               .Room));
                case ProtocolExchange.MessageOneofCase.ListRoomRequest:
                    return new Messages.ListRoomsRequest();
                case ProtocolExchange.MessageOneofCase.ListRoomResponse:
                    return new Messages.ListRoomsResponse(protocolExchange.ListRoomResponse.Rooms
                                                                          .Select(ChatRoom.FromProtobuf)
                                                                          .ToList());
                case ProtocolExchange.MessageOneofCase.JoinRoomRequest:
                    return new Messages.JoinRoomRequest(protocolExchange.JoinRoomRequest.RoomId);
                case ProtocolExchange.MessageOneofCase.JoinRoomResponse:
                    return new Messages.JoinRoomResponse(ChatRoom.FromProtobuf(protocolExchange.JoinRoomResponse.Room),
                                                         protocolExchange.JoinRoomResponse.Messages
                                                                         .Select(message => message.FromProtobuf())
                                                                         .ToList());
                case ProtocolExchange.MessageOneofCase.LeaveRoomRequest:
                    return new Messages.LeaveRoomRequest(ChatRoom.FromProtobuf(protocolExchange.LeaveRoomRequest.Room));
                case ProtocolExchange.MessageOneofCase.LeaveRoomResponse:
                    return new
                        Messages.LeaveRoomResponse(ChatRoom.FromProtobuf(protocolExchange.LeaveRoomResponse.Room));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
