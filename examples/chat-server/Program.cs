using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Critical.Chat.Server.Examples.TCP
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
