using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using XpAndRepBot.Database;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class MeCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        var userId = ExtractUserId(update.Message, out var flag);
        if (userId == 0) return;

        var replyId = update.Message.ReplyToMessage?.MessageId ?? update.Message.MessageId;
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "backmw"),
                InlineKeyboardButton.WithCallbackData("Вперёд", "nextmw"),
            }
        });

        await botClient.SendTextMessageAsync(update.Message.Chat.Id, await MeHandler.Me(userId, update.Message.Chat.Id),
            cancellationToken, replyId, markup: flag ? null : inlineKeyboard);
    }

    private static long ExtractUserId(Message message, out bool flag)
    {
        if (message.Entities == null)
        {
            flag = false;
            return 0;
        }

        flag = message.Entities.Any(x => x.Type is MessageEntityType.TextMention or MessageEntityType.Mention);
        if (flag)
        {
            foreach (var entity in message.Entities)
            {
                switch (entity.Type)
                {
                    case MessageEntityType.TextMention:
                        return entity.User?.Id ?? 0;
                    case MessageEntityType.Mention:
                    {
                        using var db = new DbUsersContext();
                        var user = db.Users.FirstOrDefault(x => x.ChatId == message.Chat.Id &&
                                                                x.Username == message.Text.Substring(
                                                                    entity.Offset + 1,
                                                                    entity.Length - 1));
                        return user?.UserId ?? 0;
                    }
                }
            }
        }


        if (message.ReplyToMessage is { From.IsBot: false } &&
            message.ReplyToMessage.From.Id != 777000)
        {
            return message.ReplyToMessage.From.Id;
        }

        return message.From?.Id ?? 0;
    }
}