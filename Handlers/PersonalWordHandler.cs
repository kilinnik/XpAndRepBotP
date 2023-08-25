using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public class PersonalWordHandler
{
    public static async Task<string> PersonalWord(long userId, string word, long chatId)
    {
        const string query = @"
                WITH UserRankedWords AS (
                    SELECT ROW_NUMBER() OVER (ORDER BY SUM([Count]) DESC) AS RowNumber, [Word], SUM([Count]) AS WordCount 
                    FROM dbo.UserLexicons 
                    WHERE [UserId] = @UserId AND [ChatId] = @ChatId 
                    GROUP BY [Word]
                )
                SELECT RowNumber, [Word], WordCount 
                FROM UserRankedWords 
                WHERE [Word] = @Word;
            ";

        await using SqlConnection connection = new(Constants.ConnectionString);
        await connection.OpenAsync();

        var wordData =
            await connection.QueryFirstOrDefaultAsync(query, new { UserId = userId, ChatId = chatId, Word = word });

        if (wordData == null) return $"Слово {word} ни разу не употреблялось";

        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(x => x.UserId == userId);
        return
            $"✍🏿 {user?.Name} употреблял слово {word} {wordData.WordCount} раз. Оно занимает {wordData.RowNumber} место по частоте употребления";
    }
}