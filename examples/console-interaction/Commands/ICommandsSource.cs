using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Client.Examples.Commands
{
    public interface ICommandsSource
    {
        Task<ICommand> NextCommand(CancellationToken token = default);
    }
}
