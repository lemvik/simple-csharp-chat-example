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
        JoinRoom,
        LeaveRoom,
        SendMessage,
        ReceiveMessage
    }
}
