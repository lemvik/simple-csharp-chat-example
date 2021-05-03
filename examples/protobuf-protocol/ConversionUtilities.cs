using System;
using System.Linq;
using Lemvik.Example.Chat.Protocol.Messages;

namespace Lemvik.Example.Chat.Protocol.Protobuf
{
    internal static class ConversionUtilities
    {
        private static Protocol.ChatUser FromProtobuf(this ChatUser user)
        {
            return new Protocol.ChatUser(user.UserId, user.UserName);
        }

        private static Protocol.ChatRoom FromProtobuf(this ChatRoom chatRoom)
        {
            return new Protocol.ChatRoom(chatRoom.Id, chatRoom.Name);
        }

        private static ChatMessage FromProtobuf(this UserMessage message)
        {
            return new ChatMessage(message.User.FromProtobuf(),
                                   message.Room.FromProtobuf(),
                                   message.Message);
        }

        private static ChatRoom ToProtobuf(this Protocol.ChatRoom chatRoom)
        {
            return new ChatRoom
            {
                Id = chatRoom.Id,
                Name = chatRoom.Name
            };
        }

        private static ChatUser ToProtobuf(this Protocol.ChatUser chatUser)
        {
            return new ChatUser
            {
                UserId = chatUser.Id,
                UserName = chatUser.Name
            };
        }

        private static UserMessage ToProtobuf(this ChatMessage chatMessage)
        {
            return new UserMessage
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
                                listRoomsResponse.Rooms.Select(room => new ChatRoom
                                {
                                    Id = room.Id,
                                    Name = room.Name
                                })
                            }
                        }
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
                case Messages.HandshakeRequest handshakeRequest:
                    return new ProtocolMessage
                        {HandshakeRequest = new HandshakeRequest {User = handshakeRequest.User.ToProtobuf()}};
                case Messages.HandshakeResponse _:
                    return new ProtocolMessage {HandshakeResponse = new HandshakeResponse()};
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        internal static IMessage FromProtocolMessage(this ProtocolMessage message)
        {
            switch (message.MessageCase)
            {
                case ProtocolMessage.MessageOneofCase.ExchangeMessage:
                    return message.ExchangeMessage.FromProtobuf();
                case ProtocolMessage.MessageOneofCase.UserMessage:
                    return message.UserMessage.FromProtobuf();
                case ProtocolMessage.MessageOneofCase.HandshakeRequest:
                    return new Messages.HandshakeRequest(message.HandshakeRequest.User.FromProtobuf());
                case ProtocolMessage.MessageOneofCase.HandshakeResponse:
                    return new Messages.HandshakeResponse();
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
                case ProtocolExchange.MessageOneofCase.CreateRoomRequest:
                    return new Messages.CreateRoomRequest(protocolExchange.CreateRoomRequest.RoomName);
                case ProtocolExchange.MessageOneofCase.CreateRoomResponse:
                    return new Messages.CreateRoomResponse(protocolExchange.CreateRoomResponse.Room.FromProtobuf());
                case ProtocolExchange.MessageOneofCase.ListRoomRequest:
                    return new Messages.ListRoomsRequest();
                case ProtocolExchange.MessageOneofCase.ListRoomResponse:
                    return new Messages.ListRoomsResponse(protocolExchange.ListRoomResponse.Rooms
                                                                          .Select(room => room.FromProtobuf())
                                                                          .ToList());
                case ProtocolExchange.MessageOneofCase.JoinRoomRequest:
                    return new Messages.JoinRoomRequest(protocolExchange.JoinRoomRequest.RoomId);
                case ProtocolExchange.MessageOneofCase.JoinRoomResponse:
                    return new Messages.JoinRoomResponse(protocolExchange.JoinRoomResponse.Room.FromProtobuf(),
                                                         protocolExchange.JoinRoomResponse.Messages
                                                                         .Select(message => message.FromProtobuf())
                                                                         .ToList());
                case ProtocolExchange.MessageOneofCase.LeaveRoomRequest:
                    return new Messages.LeaveRoomRequest(protocolExchange.LeaveRoomRequest.Room.FromProtobuf());
                case ProtocolExchange.MessageOneofCase.LeaveRoomResponse:
                    return new
                        Messages.LeaveRoomResponse(protocolExchange.LeaveRoomResponse.Room.FromProtobuf());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
