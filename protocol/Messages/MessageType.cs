namespace Critical.Chat.Protocol.Messages
{
    public enum MessageType
    {
        ListRoomsRequest,
        ListRoomsResponse,
        CreateRoom,
        JoinRoom,
        LeaveRoom,
        SendMessage,
        ReceiveMessage
    }
}
