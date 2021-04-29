namespace Critical.Chat.Client.Example.TCP
{
    public class ClientConfig
    {
        public class ServerAddress
        {
            public string Host { get; set; }
            public int Port { get; set; }
        }
        
        public ServerAddress Server { get; set; }
    }
}
