namespace Lemvik.Example.Chat.Protocol
{
    public class ChatUser
    {
        public string Id { get; }
        public string Name { get; }
        
        public ChatUser(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"ChatUser[Id={Id},Name={Name}]";
        }
    }
}
