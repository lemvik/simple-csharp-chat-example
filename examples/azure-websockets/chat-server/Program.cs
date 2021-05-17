using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    internal static class Program
    {
        private static Task Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(configure =>
                       {
                           configure.UseStartup<Startup>();
                       })
                       .RunConsoleAsync();
        }
    }
}
