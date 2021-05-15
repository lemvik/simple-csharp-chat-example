using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    internal static class Program
    {
        private static Task Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureServices((builder, services) =>
                       {
                           services.AddChatServer()
                                   .AddServerLogging()
                                   .AddConfiguration(builder.Configuration);
                       })
                       .RunConsoleAsync();
        }
    }
}
