using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lemvik.Example.Chat.Client.Example.TCP.Commands;
using Lemvik.Example.Chat.Shared;
using Microsoft.Extensions.Logging;

namespace Lemvik.Example.Chat.Client.Example.TCP
{
    public class ConsoleCommandsReader : ICommandsSource, IAsyncRunnable
    {
        private readonly ILogger logger;
        private readonly Channel<string> transferChannel;
        private readonly CancellationTokenSource applicationLifetime;

        public ConsoleCommandsReader(ILogger logger)
        {
            this.logger = logger;
            this.applicationLifetime = new CancellationTokenSource();
            this.transferChannel = Channel.CreateUnbounded<string>();
        }

        public Task RunAsync(CancellationToken token = default)
        {
            token.Register(applicationLifetime.Cancel);
            // Input task has to run in separate _thread_ for sure, otherwise tasks that are awaited as part of
            // processing input (via sequence of awaits) will end up blocking on Console.In.ReadLineAsync()
            // that is not really async as per docs.
            return Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var input = await Console.In.ReadLineAsync();
                    if (input != null)
                    {
                        await transferChannel.Writer.WriteAsync(input, token);
                    }
                }
            }, token);
        }

        public async Task<ICommand> NextCommand(CancellationToken token = default)
        {
            var operationToken = CancellationTokenSource.CreateLinkedTokenSource(
                 applicationLifetime.Token,
                 token
                ).Token;

            ICommand command = null;

            while (command == null && !token.IsCancellationRequested)
            {
                var commandLine = await transferChannel.Reader.ReadAsync(operationToken);

                command = Parse(commandLine);

                if (command == null)
                {
                    logger.LogWarning("Failed to parse command: {Line}", commandLine);
                }
            }

            return command;
        }

        private static ICommand Parse(string command)
        {
            var trimmed = command.Trim();
            // TODO: something saner. 
            if (trimmed.Equals(":l"))
            {
                return new ListRoomsCommand();
            }

            if (trimmed.Equals(":i"))
            {
                return new InfoCommand();
            }

            if (trimmed.StartsWith(":j"))
            {
                var roomName = trimmed[2..].Trim();
                return new JoinRoomCommand(roomName);
            }

            if (trimmed.StartsWith(":s"))
            {
                var arguments = trimmed[2..]
                    .Split(" ", 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (arguments.Length == 2)
                {
                    var roomName = arguments[0];
                    var message = arguments[1];
                    return new SendMessageCommand(roomName, message);
                }
            }

            return null;
        }
    }
}
