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

        private static ProtocolMessage ToProtocolMessage(this Messages.ListRoomsRequest listRoomsRequest)
        {
            return new ProtocolMessage {Id = listRoomsRequest.RequestId, ListRoomRequest = new ListRoomsRequest()};
        }

        private static ProtocolMessage ToProtocolMessage(this Messages.ListRoomsResponse listRoomsResponse)
        {
            return new ProtocolMessage()
            {
                Id = listRoomsResponse.RequestId,
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

        private static ProtocolMessage ToProtocolMessage(this Messages.HandshakeRequest handshakeRequest)
        {
            return new ProtocolMessage()
            {
                Id = handshakeRequest.Id,
                HandshakeRequest = new HandshakeRequest()
                {
                    User = handshakeRequest.User.ToProtobuf()
                }
            };
        }

        private static ProtocolMessage ToProtocolMessage(this Messages.HandshakeResponse handshakeResponse)
        {
            return new ProtocolMessage()
            {
                Id = handshakeResponse.Id,
                HandshakeResponse = new HandshakeResponse()
            };
        }

        private static ProtocolMessage ToProtocolMessage(this Messages.CreateRoomRequest createRoomRequest)
        {
            return new ProtocolMessage()
            {
                Id = createRoomRequest.RequestId,
                CreateRoomRequest = new CreateRoomRequest()
                {
                    RoomName = createRoomRequest.RoomName
                }
            };
        }

        private static ProtocolMessage ToProtocolMessage(this Messages.CreateRoomResponse createRoomResponse)
        {
            return new ProtocolMessage()
            {
                Id = createRoomResponse.RequestId,
                CreateRoomResponse = new CreateRoomResponse()
                {
                    Room = createRoomResponse.Room.ToProtobuf()
                }
            };
        }

        private static ProtocolMessage ToProtocolMessage(this Messages.JoinRoomRequest joinRoomRequest)
        {
            return new ProtocolMessage()
            {
                Id = joinRoomRequest.RequestId,
                JoinRoomRequest = new JoinRoomRequest()
                {
                    RoomId = joinRoomRequest.RoomId
                }
            };
        }

        private static ProtocolMessage ToProtocolMessage(this Messages.JoinRoomResponse joinRoomResponse)
        {
            return new ProtocolMessage()
            {
                Id = joinRoomResponse.RequestId,
                JoinRoomResponse = new JoinRoomResponse()
                {
                    Room = joinRoomResponse.Room.ToProtobuf(),
                    Messages =
                    {
                        joinRoomResponse.Messages.Select(message => message.ToProtobuf())
                    }
                }
            };
        }

        private static ProtocolMessage ToProtocolMessage(this Messages.LeaveRoomRequest leaveRoomRequest)
        {
            return new ProtocolMessage()
            {
                Id = leaveRoomRequest.RequestId,
                LeaveRoomRequest = new LeaveRoomRequest()
                {
                    Room = leaveRoomRequest.Room.ToProtobuf()
                }
            };
        }

        private static ProtocolMessage ToProtocolMessage(this Messages.LeaveRoomResponse leaveRoomResponse)
        {
            return new ProtocolMessage()
            {
                Id = leaveRoomResponse.RequestId,
                LeaveRoomResponse = new LeaveRoomResponse()
                {
                    Room = leaveRoomResponse.Room.ToProtobuf()
                }
            };
        }

        internal static ProtocolMessage ToProtocolMessage(this IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.ListRoomsRequest:
                    return message.Cast<Messages.ListRoomsRequest>().ToProtocolMessage();
                case MessageType.ListRoomsResponse:
                    return message.Cast<Messages.ListRoomsResponse>().ToProtocolMessage();
                case MessageType.HandshakeRequest:
                    return message.Cast<Messages.HandshakeRequest>().ToProtocolMessage();
                case MessageType.HandshakeResponse:
                    return message.Cast<Messages.HandshakeResponse>().ToProtocolMessage();
                case MessageType.CreateRoomRequest:
                    return message.Cast<Messages.CreateRoomRequest>().ToProtocolMessage();
                case MessageType.CreateRoomResponse:
                    return message.Cast<Messages.CreateRoomResponse>().ToProtocolMessage();
                case MessageType.JoinRoomRequest:
                    return message.Cast<Messages.JoinRoomRequest>().ToProtocolMessage();
                case MessageType.JoinRoomResponse:
                    return message.Cast<Messages.JoinRoomResponse>().ToProtocolMessage();
                case MessageType.LeaveRoomRequest:
                    return message.Cast<Messages.LeaveRoomRequest>().ToProtocolMessage();
                case MessageType.LeaveRoomResponse:
                    return message.Cast<Messages.LeaveRoomResponse>().ToProtocolMessage();
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

        private static IChatMessage FromProtobuf(UserMessage message)
        {
            return new ChatMessage(ChatUser.FromProtobuf(message.User),
                ChatRoom.FromProtobuf(message.Room),
                message.Message);
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
                    return new Messages.HandshakeRequest(message.Id,
                        ChatUser.FromProtobuf(message.HandshakeRequest.User));
                case ProtocolMessage.MessageOneofCase.HandshakeResponse:
                    return new Messages.HandshakeResponse(message.Id);
                case ProtocolMessage.MessageOneofCase.CreateRoomRequest:
                    return new Messages.CreateRoomRequest(message.Id, message.CreateRoomRequest.RoomName);
                case ProtocolMessage.MessageOneofCase.CreateRoomResponse:
                    return new Messages.CreateRoomResponse(message.Id,
                        ChatRoom.FromProtobuf(message.CreateRoomResponse.Room));
                case ProtocolMessage.MessageOneofCase.JoinRoomRequest:
                    return new Messages.JoinRoomRequest(message.Id, message.JoinRoomRequest.RoomId);
                case ProtocolMessage.MessageOneofCase.JoinRoomResponse:
                    return new Messages.JoinRoomResponse(message.Id,
                        ChatRoom.FromProtobuf(message.JoinRoomResponse.Room),
                        message.JoinRoomResponse.Messages.Select(FromProtobuf).ToList());
                case ProtocolMessage.MessageOneofCase.LeaveRoomRequest:
                    return new Messages.LeaveRoomRequest(message.Id,
                        ChatRoom.FromProtobuf(message.LeaveRoomResponse.Room));
                case ProtocolMessage.MessageOneofCase.LeaveRoomResponse:
                    return new Messages.LeaveRoomResponse(message.Id,
                        ChatRoom.FromProtobuf(message.LeaveRoomResponse.Room));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
