using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace XpAndRepBot.Commands;

public class LinkCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Скачать репак", "link_repack") },
            new[] { InlineKeyboardButton.WithCallbackData("Чат в ТГ", "link_chat") },
            new[] { InlineKeyboardButton.WithCallbackData("YouTube", "link_youtube") },
            new[] { InlineKeyboardButton.WithCallbackData("Discord", "link_discord") },
            new[] { InlineKeyboardButton.WithCallbackData("ВКонтакте", "link_vk") },
            new[] { InlineKeyboardButton.WithCallbackData("ВКонтакте (запасной паблик)", "link_vk_reserve") },
            new[] { InlineKeyboardButton.WithCallbackData("ТикТок", "link_tiktok") },
            new[] { InlineKeyboardButton.WithCallbackData("NITOKIN SOFT", "link_soft") },
            new[] { InlineKeyboardButton.WithCallbackData("Nitokin - apps", "link_apps") },
            new[] { InlineKeyboardButton.WithCallbackData("Скрыть", "link_hide") }
        });

        if (update.Message != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Список ссылок:", cancellationToken,
                    update.Message.MessageId, markup: inlineKeyboard);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Список ссылок:", cancellationToken,
                    markup: inlineKeyboard);
            }
        }
    }
}