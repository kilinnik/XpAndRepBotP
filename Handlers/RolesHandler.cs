using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public static class RolesHandler
{
    public static string Roles(int number, long chatId)
    {
        using var db = new DbUsersContext();
        var roleUsers = db.Users
            .Where(u => u.Roles != null && u.ChatId == chatId)
            .Select(u => new { u.Roles, u.Name })
            .ToList();

        var roles = roleUsers
            .SelectMany(u => u.Roles.Split(", ", StringSplitOptions.RemoveEmptyEntries))
            .Distinct()
            .OrderBy(r => r)
            .Skip(number)
            .Take(20)
            .ToList();

        var sb = new StringBuilder();
        foreach (var role in roles)
        {
            var users = roleUsers
                .Where(u => u.Roles.StartsWith(role) || u.Roles.Contains(", " + role))
                .Select(u => u.Name);

            sb.AppendLine($"{Utilities.NumberToEmoji(++number)} || <code>{role}</code>: {string.Join(", ", users)}");
        }

        return sb.ToString();
    }

    public static async Task HandleRolesCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        var match = Utilities.GetMatchFromMessage(callbackQuery.Message, @"^((?:\d️⃣)+)");
        var number = match is { Success: true } ? Utilities.EmojiToNumber(match.Value) : 0;
        var isBackward = callbackQuery.Data == "backr";
        var offset = isBackward ? -21 : 19;
        if (isBackward && number <= 20) return;

        if (callbackQuery.Message != null)
        {
            var newText = Roles(number + offset, callbackQuery.Message.Chat.Id);
            var inlineKeyboard = Utilities.CreateInlineKeyboard("backr", "nextr");
            await botClient.EditMessageTextAsync(callbackQuery.Message, newText, inlineKeyboard, cancellationToken: cancellationToken);
        }
    }
}