namespace Lemvik.Example.Chat.Protocol
{
    public class ChatRoom
    {
        public string Id { get; }
        public string Name { get; }
        
        public ChatRoom(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"ChatRoom[Id={Id},Name={Name}]";
        }
    }
}
