using Domain.DTO;

namespace Bll.Interfaces;

public interface ITelegramMessageService
{
    Task SendMessageAsync(CommandResult commandResult, CancellationToken token = default);
}