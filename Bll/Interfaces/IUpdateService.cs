using Telegram.Bot.Types;

namespace Bll.Interfaces;

public interface IUpdateService
{
    Task HandleUpdateAsync(Update update, CancellationToken token);
    
    Task HandleErrorAsync(Exception exception);
}