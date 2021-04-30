namespace Critical.Chat.Client.Example.TCP
{
    public class ClientConfig
    {
        public class ServerAddress
        {
            public string Host { get; set; }
            public int Port { get; set; }
        }

        public class ChatClientConfig : IChatClientConfiguration
        {
            public string UserName { get; set; }
        }
        
        public ServerAddress Server { get; set; }
        public ChatClientConfig ChatConfig { get; set; }
    }
}
