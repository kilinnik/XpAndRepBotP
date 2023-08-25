using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class TopLexiconCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "backl"),
                InlineKeyboardButton.WithCallbackData("Вперёд", "nextl"),
            }
        });

        if (update.Message != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                    await TopLexiconHandler.TopLexicon(0, update.Message.Chat.Id), cancellationToken,
                    update.Message.MessageId, markup: inlineKeyboard);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                    await TopLexiconHandler.TopLexicon(0, update.Message.Chat.Id), cancellationToken,
                    markup: inlineKeyboard);
            }
        }
    }
}