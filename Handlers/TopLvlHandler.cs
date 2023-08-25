using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public static class TopLvlHandler
{
    public static string TopLvl(int number, long chatId)
    {
        using var db = new DbUsersContext();
        var users = db.Users.Where(x => x.ChatId == chatId).OrderByDescending(x => x.Lvl)
            .ThenByDescending(y => y.CurXp).Skip(number).Take(50);
        StringBuilder sb = new("🏆 \n");
        var i = number + 1;
        foreach (var user in users)
        {
            sb.AppendLine(
                $"{Utilities.NumberToEmoji(i)} {user.Name} lvl {user.Lvl}({user.CurXp}/{Utilities.GenerateXpForLevel(user.Lvl + 1)})");
            i++;
        }

        return sb.ToString();
    }

    public static async Task HandleTopLvlCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        var match = Utilities.GetMatchFromMessage(callbackQuery.Message, @"(?<=^[^\n]*\n)((?:\d️⃣)+)");
        var number = match is { Success: true } ? Utilities.EmojiToNumber(match.Value) : 0;
        var isBackward = callbackQuery.Data == "backtl";
        var offset = isBackward ? -51 : 49;
        if (isBackward && number <= 50) return;

        if (callbackQuery.Message != null)
        {
            var newText = TopLvl(number + offset, callbackQuery.Message.Chat.Id);
            var inlineKeyboard = Utilities.CreateInlineKeyboard("backtl", "nexttl");
            await botClient.EditMessageTextAsync(callbackQuery.Message, newText, inlineKeyboard, cancellationToken: cancellationToken);
        }
    }
}