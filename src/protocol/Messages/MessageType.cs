namespace Critical.Chat.Protocol.Messages
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
        SendMessage,
        ReceiveMessage
    }
}
