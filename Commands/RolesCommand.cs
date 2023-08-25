using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class RolesCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "backr"),
                InlineKeyboardButton.WithCallbackData("Вперёд", "nextr"),
            }
        });

        if (update.Message != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                    RolesHandler.Roles(0, update.Message.Chat.Id), cancellationToken, update.Message.MessageId,
                    ParseMode.Html, inlineKeyboard);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                    RolesHandler.Roles(0, update.Message.Chat.Id), cancellationToken,
                    parseMode: ParseMode.Html, markup: inlineKeyboard);
            }
        }
    }
}