using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class MarriageCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        var replyMessage = update.Message.ReplyToMessage;
        if (replyMessage?.From == null || replyMessage.From.IsBot || replyMessage.From.Id == 777000)
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                "Ответьте на сообщение того, с кем хотите заключить брак", cancellationToken,
                update.Message.MessageId);
            return;
        }

        await using var db = new DbUsersContext();
        var user1 = db.Users.FirstOrDefault(x =>
            x.UserId == update.Message.From.Id && x.ChatId == update.Message.Chat.Id);
        var user2 = db.Users.FirstOrDefault(x =>
            x.UserId == replyMessage.From.Id && x.ChatId == update.Message.Chat.Id);

        if (user1?.Mariage != 0)
        {
            if (user1 != null)
            {
                await botClient.SendTextMessageAsync(user1.ChatId, "Вы уже в браке", cancellationToken);
            }

            return;
        }

        if (user2?.Mariage != 0)
        {
            if (user2 != null)
            {
                await botClient.SendTextMessageAsync(user1.ChatId, $"{user2.Name} уже в браке", cancellationToken,
                    update.Message.MessageId);
            }

            return;
        }

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Нет", $"mny{user1.UserId}"),
                InlineKeyboardButton.WithCallbackData("Да", $"my{user1.UserId}"),
            }
        });

        await botClient.SendTextMessageAsync(update.Message.Chat.Id,
            $"💖 {user1.Name} делает вам предложение руки и сердца. Согласны ли вы вступить в брак с {user1.Name}?",
            cancellationToken, replyMessage.MessageId, markup: inlineKeyboard);
    }
}