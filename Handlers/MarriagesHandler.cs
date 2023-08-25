using System;
using System.Linq;
using System.Text;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public class MarriagesHandler
{
    public static string Marriages(long chatId)
    {
        using var db = new DbUsersContext();
        var users = db.Users
            .Where(x => x.Mariage != 0 && x.ChatId == chatId)
            .OrderBy(y => y.DateMariage)
            .ToList();

        StringBuilder sb = new();
        sb.Append("💒 список браков: \n");
        var number = 1;

        for (var i = 0; i < users.Count; i++)
        {
            var ts = DateTime.Now - users[i].DateMariage;
            var formattedDate = users[i].DateMariage.ToString("yy/MM/dd HH:mm:ss");

            string partnerName = null;

            if (users[i].UserId == users[i].Mariage)
            {
                partnerName = users[i].Name;
            }
            else if (i < users.Count - 1 && users[i + 1].UserId == users[i].Mariage)
            {
                partnerName = users[i + 1].Name;
            }

            if (partnerName == null) continue;
            sb.AppendLine(FormatMarriageLine(number, users[i].Name, partnerName, formattedDate, ts));
            number++;
        }

        return sb.ToString();
    }

    private static string FormatMarriageLine(int number, string name1, string name2, string formattedDate, TimeSpan ts)
    {
        return $"{Utilities.NumberToEmoji(number)} {name1} и {name2} c {formattedDate} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m";
    }
}