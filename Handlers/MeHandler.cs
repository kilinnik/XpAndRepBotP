using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using XpAndRepBot.Database;
using XpAndRepBot.Models;

namespace XpAndRepBot.Handlers;

public static class MeHandler
{
    public static async Task<string> Me(long userId, long chatId)
    {
        await using var db = new DbUsersContext();
        var user = db.Users.First(x => x.UserId == userId && x.ChatId == chatId);
        var result = new StringBuilder($"👨‍❤️‍👨 Имя: {user.Name}" +
                                       $"\n🕰 Время последнего сообщения: {user.TimeLastMes:yy/MM/dd HH:mm:ss}" +
                                       $"\n⭐️ Lvl: {user.Lvl}({user.CurXp}/{Utilities.GenerateXpForLevel(user.Lvl + 1)})" +
                                       $"\n🎭 Роли: {user.Roles}" +
                                       $"\n🏆 Место в топе по уровню: {Utilities.PlaceLvl(user.UserId, db.Users, chatId)}" +
                                       $"\n😇 Rep: {user.Rep}" +
                                       $"\n🥇 Место в топе по репутации: {Utilities.PlaceRep(user.UserId, db.Users, chatId)}" +
                                       $"\n🎖 Место в топе по лексикону: {await Utilities.PlaceLexicon(user, chatId)}" +
                                       $"\n🤬 Кол-во варнов: {user.Warns}/3" +
                                       $"\n🗓 Дата последнего варна/снятия варна: {user.LastTime:yyyy-MM-dd}\n");

        await using var connection = new SqlConnection(Constants.ConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT COUNT(*) FROM dbo.UserLexicons WHERE [UserId] = @userId AND [ChatId] = @chatId", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@chatId", chatId);
        var count = (int)(await command.ExecuteScalarAsync())!;
        result.AppendLine($"🔤 Лексикон: {count} слов");
        result.AppendLine("📖 Личный топ слов:");

        await using var command2 = new SqlCommand(
            "SELECT TOP 10 * FROM dbo.UserLexicons WHERE [UserId] = @userId2 AND [ChatId] = @chatId2 ORDER BY [Count] DESC",
            connection);
        command2.Parameters.AddWithValue("@userId2", userId);
        command2.Parameters.AddWithValue("@chatId2", chatId);
        await using var reader = await command2.ExecuteReaderAsync();

        var words = new List<Words>();
        while (await reader.ReadAsync())
        {
            var word = new Words
            {
                Word = reader.GetString(1),
                Count = reader.GetInt32(2)
            };
            words.Add(word);
        }

        for (var i = 0; i < words.Count; i++)
        {
            result.AppendLine($"{Constants.KeycodeKeymaps[i + 1]} {words[i].Word} || {words[i].Count}");
        }

        return result.ToString();
    }
}