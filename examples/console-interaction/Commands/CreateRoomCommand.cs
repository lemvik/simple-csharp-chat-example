namespace Lemvik.Example.Chat.Client.Examples.Commands
{
    public class CreateRoomCommand : ICommand
    {
        public string RoomName { get; } 
        
        public CreateRoomCommand(string roomName)
        {
            RoomName = roomName;
        }
    }
}
