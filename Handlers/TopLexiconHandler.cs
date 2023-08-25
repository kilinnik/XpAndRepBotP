using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;
using XpAndRepBot.Models;

namespace XpAndRepBot.Handlers;

public static class TopLexiconHandler
{
    public static async Task<string> TopLexicon(int number, long chatId)
    {
        const string query =
            @"SELECT ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC) AS RowNumber, UserID, COUNT(*) AS UserCount 
                  FROM dbo.UserLexicons WHERE [ChatId] = @ChatId 
                  GROUP BY UserID ORDER BY UserCount DESC OFFSET @Number ROWS FETCH NEXT 50 ROWS ONLY";

        await using SqlConnection connection = new(Constants.ConnectionString);
        await connection.OpenAsync();

        var result = new StringBuilder("🎖 Топ по лексикону:\n");
        var users = await connection.QueryAsync<UserLexiconData>(query, new { ChatId = chatId, Number = number });

        await using var db = new DbUsersContext();
        foreach (var user in users)
        {
            var userName = db.Users.FirstOrDefault(x => x.UserId == user.UserId && x.ChatId == chatId)?.Name;
            result.Append($"{Utilities.NumberToEmoji(user.RowNumber)} {userName} || {user.UserCount}\n");
        }

        return result.ToString();
    }

    public static async Task HandleTopLexiconCallbackQuery(ITelegramBotClient botClient,
        CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var match = Utilities.GetMatchFromMessage(callbackQuery.Message, @"(?<=^[^\n]*\n)((?:\d️⃣)+)");
        var number = match is { Success: true } ? Utilities.EmojiToNumber(match.Value) : 0;
        var isBackward = callbackQuery.Data == "backl";
        var offset = isBackward ? -51 : 49;
        if (isBackward && number <= 50) return;

        if (callbackQuery.Message != null)
        {
            var newText = await TopLexicon(number + offset, callbackQuery.Message.Chat.Id);
            var inlineKeyboard = Utilities.CreateInlineKeyboard("backl", "nextl");
            await botClient.EditMessageTextAsync(callbackQuery.Message, newText, inlineKeyboard, cancellationToken: cancellationToken);
        }
    }
}