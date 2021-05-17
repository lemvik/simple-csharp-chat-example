using System;
using System.Net;
using System.Threading.Tasks;
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

            appBuilder.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60),
            });
            appBuilder.UseRouting();

            appBuilder.UseEndpoints(endpointsBuilder =>
            {
                endpointsBuilder.MapGet("/health", async requestContext =>
                {
                    await requestContext.Response.WriteAsync("OK");
                });

                endpointsBuilder.Map("/ws", async requestContext =>
                {
                    if (!requestContext.WebSockets.IsWebSocketRequest)
                    {
                        requestContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    }

                    var socket = await requestContext.WebSockets.AcceptWebSocketAsync();
                    
                    var server = requestContext.RequestServices.GetRequiredService<ChatServer>();

                    await server.AcceptWebSocket(requestContext, socket);
                });
            });
        }
    }
}
