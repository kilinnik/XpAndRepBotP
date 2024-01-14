using Telegram.Bot.Types;

namespace Bll.Interfaces;

public interface ICommandService
{
    Task ExecuteCommandAsync(Message message, CancellationToken token);
}