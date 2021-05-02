namespace Lemvik.Example.Chat.Server.Examples.TCP
{
    public class ServerConfig
    {
        public class ListeningConfig
        {
            public string Host { get; set; }
            public int Port { get; set; } 
        }
        
        public ListeningConfig Listening { get; set; } 
    }
}
