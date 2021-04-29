using System;
using Critical.Chat.Server.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Critical.Chat.Server.Examples.TCP
{
    internal static class ServerRegistrations
    {
        internal static IServiceCollection AddChatServer(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<IServerRoomsRegistry, ServerRoomsRegistry>()
                .AddSingleton<IHostedService, ChatServer>();
        }

        internal static IServiceCollection AddServerLogging(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddLogging(logging => logging.AddConsole());
        }

        internal static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection,
                                                            IConfiguration configuration)
        {
            return serviceCollection.Configure<ServerConfig>(configuration.GetSection(nameof(ServerConfig)));
        }
    }
}
