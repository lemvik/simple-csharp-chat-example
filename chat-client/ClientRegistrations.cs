using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Critical.Chat.Client.Example.TCP
{
    internal static class ClientRegistrations
    {
        internal static IServiceCollection AddChatClient(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IHostedService, ChatClient>();
        }
        
        internal static IServiceCollection AddClientLogging(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddLogging(logging => logging.AddConsole());
        }

        internal static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection,
                                                            IConfiguration configuration)
        {
            return serviceCollection.Configure<ClientConfig>(configuration.GetSection(nameof(ClientConfig)));
        }
    }
}
