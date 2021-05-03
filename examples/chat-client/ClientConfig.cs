using JetBrains.Annotations;

namespace Lemvik.Example.Chat.Client.Example.TCP
{
    public class ClientConfig
    {
        [UsedImplicitly]
        public class ServerAddress
        {
            public string Host { get; [UsedImplicitly] set; }
            public int Port { get; [UsedImplicitly] set; }
        }

        public ServerAddress Server { get; [UsedImplicitly] set; }
    }
}
