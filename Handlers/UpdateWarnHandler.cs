using System.Linq;
using Telegram.Bot.Types;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public static class UpdateWarnHandler
{
    public static string UpdateWarn(Update update, bool increase)
    {
        using var db = new DbUsersContext();
        if (update.Message?.ReplyToMessage?.From == null) return null;
        var userId = update.Message.ReplyToMessage.From.Id;
        var user = db.Users.FirstOrDefault(x => x.UserId == userId && x.ChatId == update.Message.Chat.Id);
        if (user == null) return null;

        if (increase)
        {
            user.Warns++;
            db.SaveChangesAsync();
            return $"{user.Name} получает предупреждение({user.Warns}/3)";
        }

        user.Warns--;

        db.SaveChangesAsync();
        return $"С {user.Name} снимается 1 варн({user.Warns}/3)";
    }
}