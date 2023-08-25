using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public static class MentionHandler
{
    public static async Task HandleMentions(DbUsersContext db, Users user, Update update, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        var messageText = Utilities.GetMessageText(update);
        if (update.Message == null || messageText.Length >= 100 || messageText[0] != '@') return;
        var chatId = user.ChatId;
        var mentionUsers = db.Users.Where(x => x.ChatId == chatId && (
            x.Roles.Equals(messageText.Substring(1)) || x.Roles.StartsWith(messageText.Substring(1) + ",") ||
            x.Roles.Contains(", " + messageText.Substring(1) + ",") ||
            x.Roles.EndsWith(", " + messageText.Substring(1)))).ToList();
        if (mentionUsers.Count <= 0) return;
        if (update.Message != null)
        {
            await Mention(mentionUsers, user.Name, chatId, botClient, cancellationToken);
        }
    }

    private static async Task Mention(IReadOnlyList<Users> users, string name, long chatId, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        var baseMessage = $"{name} призывает ";
        var currentMessage = baseMessage;

        for (var i = 0; i < users.Count; i++)
        {
            currentMessage += $"<a href=\"tg://user?id={users[i].UserId}\">{users[i].Name}</a> ";

            if ((i + 1) % 5 != 0 && i != users.Count - 1) continue;
            await botClient.SendTextMessageAsync(chatId, currentMessage,cancellationToken, parseMode: ParseMode.Html);
            currentMessage = baseMessage;
        }
    }
}