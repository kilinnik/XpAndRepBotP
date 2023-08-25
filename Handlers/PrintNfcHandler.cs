using System;
using System.Linq;
using System.Text;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public class PrintNfcHandler
{
    public static string PrintNfc(long chatId)
    {
        using var db = new DbUsersContext();
        var usersWithNfc = db.Users
            .Where(u => u.Nfc == true && u.ChatId == chatId)
            .OrderBy(u => u.StartNfc)
            .ToList();

        var sb = new StringBuilder("Без мата 👮‍♂️\n");
        for (var i = 0; i < usersWithNfc.Count; i++)
        {
            var ts = DateTime.Now - usersWithNfc[i].StartNfc;
            sb.AppendLine(
                $"{Utilities.NumberToEmoji(i + 1)} || {usersWithNfc[i].Name}: {ts.Days} d, {ts.Hours} h, {ts.Minutes} m.");
        }

        return sb.ToString();
    }
}