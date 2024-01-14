using Telegram.Bot;
using Telegram.Bot.Types;

namespace Infrastructure.Interfaces;

public interface IAiResponseService
{
    Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken token);
}
