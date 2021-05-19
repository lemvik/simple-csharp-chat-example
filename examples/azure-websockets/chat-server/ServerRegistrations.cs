using System.Collections.Generic;
using System.Linq;
using Lemvik.Example.Chat.Protocol.Messages;
using Lemvik.Example.Chat.Protocol.Protobuf;
using Lemvik.Example.Chat.Server.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using ChatRoom = Lemvik.Example.Chat.Protocol.ChatRoom;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    internal static class ServerRegistrations
    {
        internal static IServiceCollection AddChatServer(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                   .AddSingleton<IChatServer, Implementation.ChatServer>()
                   .AddSingleton<ICollection<ChatRoom>>(provider =>
                   {
                       var serverConf = provider.GetRequiredService<IOptions<ServerConfig>>().Value;
                       return serverConf.PredefinedRooms.Select(conf => new ChatRoom(conf.Id, conf.Name)).ToList();
                   })
                   .AddSingleton<IRoomSource, TransientRoomSource>()
                   .AddSingleton<IRoomRegistry, RoomRegistry>()
                   .AddSingleton(InMemoryMessageTracker.Factory)
                   .AddSingleton<IChatUserIdentityProvider, RandomChatUserIdentityProvider>()
                   .AddTransient<IMessageProtocol, ProtobufMessageProtocol>()
                   .AddSingleton<ChatServer>()
                   .AddSingleton<IHostedService>(provider => provider.GetRequiredService<ChatServer>())
                   .AddSingleton<IWebSocketAcceptor>(provider => provider.GetRequiredService<ChatServer>());
        }

        internal static IServiceCollection AddServerLogging(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddLogging(logging => logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.ColorBehavior = LoggerColorBehavior.Disabled;
                options.IncludeScopes = false;
            }));
        }

        internal static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection,
                                                            IConfiguration configuration)
        {
            return serviceCollection.Configure<ServerConfig>(configuration.GetSection(nameof(ServerConfig)));
        }
    }
}
