using Domain.DTO;
using Telegram.Bot.Types;

namespace Bll.Interfaces;

public interface ICommand
{
    Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token);
}
