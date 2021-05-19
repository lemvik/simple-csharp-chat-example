using JetBrains.Annotations;

namespace Lemvik.Example.Chat.Client.Examples.Azure
{
    public class ClientConfig
    {
        [UsedImplicitly]
        public class ServerAddress
        {
            public string Url { get; [UsedImplicitly] set; }
        }

        public ServerAddress Server { get; [UsedImplicitly] set; }
    }
}
