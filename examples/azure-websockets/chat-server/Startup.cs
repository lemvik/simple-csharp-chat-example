using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lemvik.Example.Chat.Server.Examples.Azure
{
    internal class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddChatServer()
                    .AddServerLogging()
                    .AddConfiguration(configuration);
        }

        public void Configure(IApplicationBuilder appBuilder, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                appBuilder.UseDeveloperExceptionPage();
            }

            appBuilder.UseRouting();

            appBuilder.UseEndpoints(endpointsBuilder =>
            {
                endpointsBuilder.MapGet("/health", async requestContext =>
                {
                    await requestContext.Response.WriteAsync("OK");
                });
            });
        }
    }
}
