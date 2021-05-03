namespace Lemvik.Example.Chat.Client.Example.TCP.Commands
{
    public class SendMessageCommand : ICommand
    {
        public string RoomName { get; } 
        public string Message { get; }
        
        public SendMessageCommand(string roomName, string message)
        {
            RoomName = roomName;
            Message = message;
        }
    }
}
