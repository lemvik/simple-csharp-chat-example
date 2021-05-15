using System.Threading;
using System.Threading.Tasks;

namespace Lemvik.Example.Chat.Client.Example.TCP.Commands
{
    public interface ICommandsSource
    {
        Task<ICommand> NextCommand(CancellationToken token = default);
    }
}
