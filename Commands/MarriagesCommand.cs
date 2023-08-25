using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class MarriagesCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                    MarriagesHandler.Marriages(update.Message.Chat.Id), cancellationToken, update.Message.MessageId);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                    MarriagesHandler.Marriages(update.Message.Chat.Id), cancellationToken);
            }
        }
    }
}