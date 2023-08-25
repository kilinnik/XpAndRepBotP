using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace XpAndRepBot.Handlers;

public class WordHandler
{
    public static async Task<string> Word(string word, long chatId)
    {
        const string query = @"
                WITH RankedWords AS (
                    SELECT ROW_NUMBER() OVER (ORDER BY SUM([Count]) DESC) AS RowNumber, [Word], SUM([Count]) AS WordCount 
                    FROM dbo.UserLexicons 
                    WHERE [ChatId] = @ChatId 
                    GROUP BY [Word]
                )
                SELECT RowNumber, [Word], WordCount 
                FROM RankedWords 
                WHERE [Word] = @Word;
            ";

        await using SqlConnection connection = new(Constants.ConnectionString);
        await connection.OpenAsync();

        var wordData = await connection.QueryFirstOrDefaultAsync(query, new { ChatId = chatId, Word = word });

        return wordData != null
            ? $"✍🏿 Слово {word} употреблялось {wordData.WordCount} раз. Оно занимает {wordData.RowNumber} место по частоте употребления"
            : $"Слово {word} ни разу не употреблялось";
    }
}