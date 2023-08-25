using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public static class TopRepHandler
{
    public static string TopRep(int number, long chatId)
    {
        using var db = new DbUsersContext();
        var users = db.Users.Where(x => x.ChatId == chatId).OrderByDescending(x => x.Rep).Skip(number)
            .Take(50);
        var resultBuilder = new StringBuilder("🥇 \n");
        var i = number + 1;
        foreach (var user in users)
        {
            resultBuilder.AppendLine($"{Utilities.NumberToEmoji(i)} {user.Name} rep {user.Rep}");
            i++;
        }

        return resultBuilder.ToString();
    }

    public static async Task HandleTopRepCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        var match = Utilities.GetMatchFromMessage(callbackQuery.Message, @"(?<=^[^\n]*\n)((?:\d️⃣)+)");
        var number = match is { Success: true } ? Utilities.EmojiToNumber(match.Value) : 0;
        var isBackward = callbackQuery.Data == "backtr";
        var offset = isBackward ? -51 : 49;
        if (isBackward && number <= 50) return;

        if (callbackQuery.Message != null)
        {
            var newText = TopRep(number + offset, callbackQuery.Message.Chat.Id);
            var inlineKeyboard = Utilities.CreateInlineKeyboard("backtr", "nexttr");
            await botClient.EditMessageTextAsync(callbackQuery.Message, newText, inlineKeyboard, cancellationToken: cancellationToken);
        }
    }
}