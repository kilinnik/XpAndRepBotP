using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public class UserWarnsHandler
{
    public static async Task HandleUserWarns(ITelegramBotClient botClient, DbUsersContext db, Users user,
        Update update, CancellationToken cancellationToken)
    {
        //remove warn
        var chat = db.Chats.FirstOrDefault(x => x.ChatId == user.ChatId);
        if (user.Warns > 0)
        {
            var difference = DateTime.Now - user.LastTime;
            if (chat != null && difference.TotalDays >= chat.WarnDays)
            {
                user.Warns--;
                user.LastTime = DateTime.Now;
                await db.SaveChangesAsync(cancellationToken);
                await NotifyUserAboutWarnRemoval(botClient, user, update, cancellationToken);
            }
        }
    }

    private static async Task NotifyUserAboutWarnRemoval(ITelegramBotClient botClient, Users user, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(user.ChatId, $"С {user.Name} снимается 1 варн({user.Warns}/3)",
                    cancellationToken, update.Message.MessageId);
            }
            catch
            {
                await botClient.SendTextMessageAsync(user.ChatId, $"С {user.Name} снимается 1 варн({user.Warns}/3)",
                    cancellationToken);
            }
        }
    }
}