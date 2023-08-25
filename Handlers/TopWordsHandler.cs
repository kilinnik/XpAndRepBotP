using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Models;

namespace XpAndRepBot.Handlers;

public static class TopWordsHandler
{
    public static async Task<string> TopWords(int number, long chatId)
    {
        const string query =
            @"SELECT ROW_NUMBER() OVER (ORDER BY SUM([Count]) DESC) AS RowNumber, [Word], SUM([Count]) AS WordCount 
                  FROM dbo.UserLexicons WHERE [ChatId] = @ChatId GROUP BY [Word] 
                  ORDER BY [WordCount] DESC OFFSET @Number ROWS FETCH NEXT 50 ROWS ONLY";

        await using SqlConnection connection = new(Constants.ConnectionString);
        await connection.OpenAsync();

        var result = new StringBuilder("📖 Топ слов:\n");
        var words = await connection.QueryAsync<WordData>(query, new { ChatId = chatId, Number = number });

        foreach (var word in words)
        {
            result.Append($"{Utilities.NumberToEmoji(word.RowNumber)} {word.Word} || {word.WordCount}\n");
        }

        return result.ToString();
    }
    
    public static async Task HandleTopWordsCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        var match = Utilities.GetMatchFromMessage(callbackQuery.Message, @"(?<=^[^\n]*\n)((?:\d️⃣)+)");
        var number = match is { Success: true } ? Utilities.EmojiToNumber(match.Value) : 0;
        var isBackward = callbackQuery.Data == "backtw";
        var offset = isBackward ? -51 : 49;
        if (isBackward && number <= 50) return;

        if (callbackQuery.Message != null)
        {
            var newText = await TopWordsHandler.TopWords(number + offset, callbackQuery.Message.Chat.Id);
            var inlineKeyboard = Utilities.CreateInlineKeyboard("backtw", "nexttw");
            await botClient.EditMessageTextAsync(callbackQuery.Message, newText, inlineKeyboard, cancellationToken: cancellationToken);
        }
    }
}