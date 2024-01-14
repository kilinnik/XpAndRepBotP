using Telegram.Bot.Types;

namespace Bll.Interfaces;

public interface ICallbackQueryHandler
{
    Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken token);
}