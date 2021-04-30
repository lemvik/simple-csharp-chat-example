namespace Critical.Chat.Protocol.Messages
{
    public enum MessageType
    {
        HandshakeRequest,
        HandshakeResponse,
        ListRoomsRequest,
        ListRoomsResponse,
        CreateRoom,
        JoinRoom,
        LeaveRoom,
        SendMessage,
        ReceiveMessage
    }
}
