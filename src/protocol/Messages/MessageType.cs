namespace Lemvik.Example.Chat.Protocol.Messages
{
    public enum MessageType
    {
        HandshakeRequest,
        HandshakeResponse,
        ListRoomsRequest,
        ListRoomsResponse,
        CreateRoomRequest,
        CreateRoomResponse,
        JoinRoomRequest,
        JoinRoomResponse,
        LeaveRoomRequest,
        LeaveRoomResponse,
        ListUsersRequest,
        ListUsersResponse,
        ChatMessage,
        SendMessage,
        ReceiveMessage
    }
}
