using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public static class AddWordsToLexiconHandler
{
    public static async Task AddWordsToLexicon(Users user, string mes)
    {
        await using SqlConnection connection = new(Constants.ConnectionString);
        await connection.OpenAsync();

        var listWords = mes.Split(new[] { " ", "\r\n", "\n" }, StringSplitOptions.None);
        var validWords = listWords
            .Select(t => Regex.Replace(t, @"[^\w\d\s]", ""))
            .Where(cleanedWord => !string.IsNullOrWhiteSpace(cleanedWord))
            .Select(cleanedWord => cleanedWord.Length > 100 ? cleanedWord[..100] : cleanedWord.ToLower())
            .ToList();

        foreach (var word in validWords)
        {
            const string updateCommandText =
                "UPDATE dbo.UserLexicons SET [Count] = [Count] + 1 WHERE [Word] = @word AND [UserId] = @userId AND [ChatId] = @chatId";
            await using SqlCommand updateCommand = new(updateCommandText, connection);
            updateCommand.Parameters.AddWithValue("@word", word);
            updateCommand.Parameters.AddWithValue("@userId", user.UserId);
            updateCommand.Parameters.AddWithValue("@chatId", user.ChatId);

            var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
            if (rowsAffected != 0) continue;
            const string insertCommandText =
                "INSERT INTO dbo.UserLexicons (UserId, Word, Count, ChatId) VALUES (@userId, @word, 1, @chatId)";
            await using SqlCommand insertCommand = new(insertCommandText, connection);
            insertCommand.Parameters.AddWithValue("@word", word);
            insertCommand.Parameters.AddWithValue("@userId", user.UserId);
            insertCommand.Parameters.AddWithValue("@chatId", user.ChatId);

            await insertCommand.ExecuteNonQueryAsync();
        }
    }
}