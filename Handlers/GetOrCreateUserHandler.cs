using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public class GetOrCreateUserHandler
{
    public static Task<Users> GetOrCreateUser(DbUsersContext db, Update update)
    {
        if (update.Message is not { From: not null }) return Task.FromResult<Users>(null);
        var idUser = update.Message.From.Id;
        var chatId = update.Message.Chat.Id;
        var user = db.Users.FirstOrDefault(x => x.UserId == idUser && x.ChatId == chatId);

        if (user == null)
        {
            var name = update.Message.From.FirstName;
            if (!string.IsNullOrEmpty(update.Message.From.LastName))
            {
                name += " " + update.Message.From.LastName;
            }

            if (name.Length > 50)
            {
                name = name[..50];
            }

            user = new Users(idUser, name, 0, 0, 0, chatId);
            db.Users.Add(user);
        }

        user.Username = update.Message.From.Username ?? user.Username;
        if (user.Name != "0") return Task.FromResult(user);
        user.Name = update.Message.From.FirstName;
        if (!string.IsNullOrEmpty(update.Message.From.LastName))
        {
            user.Name += " " + update.Message.From.LastName;
        }

        return Task.FromResult(user);
    }
}