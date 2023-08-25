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

public static class MeWordsHandler
{
    public static async Task HandleMeWordsCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        var match = Utilities.GetMatchFromMessage(callbackQuery.Message, @"^((?:\d️⃣)+)");
        var number = match is { Success: true } ? Utilities.EmojiToNumber(match.Value) : 0;
        var isBackward = callbackQuery.Data == "backmw";
        var offset = isBackward ? -10 : 10;
        if (isBackward && number <= 10) return;

        var newText = await MeWords(callbackQuery, offset, number);
        var inlineKeyboard = Utilities.CreateInlineKeyboard("backmw", "nextmw");
        await botClient.EditMessageTextAsync(callbackQuery.Message, newText, inlineKeyboard, cancellationToken: cancellationToken);
    }

    private static async Task<string> MeWords(CallbackQuery callbackQuery, int number, int curNumber)
    {
        await using var db = new DbUsersContext();
        var idUser = callbackQuery.Message?.ReplyToMessage?.From?.Id;
        var chatId = callbackQuery.Message?.Chat.Id;
        if (!idUser.HasValue || !chatId.HasValue) return null;

        var user = db.Users.First(x => x.UserId == idUser && x.ChatId == chatId);
        int i;
        var offset = 10;

        if (curNumber != 0)
        {
            offset = curNumber + number - 1;
            i = offset + 1;
        }
        else
        {
            i = 11;
        }

        if (offset == 0) return await Handlers.MeHandler.Me(idUser.Value, user.ChatId);
        await using var connection = new SqlConnection(Constants.ConnectionString);
        var result = new StringBuilder();
        var rows = await connection.QueryAsync<Words>(
            "SELECT * FROM dbo.UserLexicons WHERE [UserID] = @UserId AND [ChatId] = @ChatId ORDER BY [Count] DESC OFFSET @Offset ROWS FETCH NEXT 10 ROWS ONLY",
            new { user.UserId, user.ChatId, Offset = offset });

        rows.ToList().ForEach((word) =>
        {
            result.AppendLine($"{Utilities.NumberToEmoji(i)} {word.Word} || {word.Count}");
            i++;
        });
        return result.ToString();
    }
}