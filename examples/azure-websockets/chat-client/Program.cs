using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Lemvik.Example.Chat.Client.Examples.Azure
{
    internal static class Program
    {
        private static Task Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureServices((builder, services) =>
                       {
                           services.AddChatClient()
                                   .AddClientLogging()
                                   .AddConfiguration(builder.Configuration);
                       })
                       .RunConsoleAsync();
        }
    }
}
