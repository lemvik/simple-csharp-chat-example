namespace Lemvik.Example.Chat.Client.Example.TCP.Commands
{
    public class JoinRoomCommand : ICommand
    {
        public string RoomName { get; }

        public JoinRoomCommand(string roomName)
        {
            RoomName = roomName;
        }
    }
}
