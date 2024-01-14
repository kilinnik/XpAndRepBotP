using Domain.DTO;
using Telegram.Bot.Types;

namespace Bll.Interfaces;

public interface IUserMessageStrategy
{
    Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token);
}