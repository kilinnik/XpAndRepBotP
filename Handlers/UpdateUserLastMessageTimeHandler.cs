using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public class UpdateUserLastMessageTimeHandler
{
    public static async Task UpdateUserLastMessageTime(DbContext db, Users user, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            user.TimeLastMes = update.Message.Date.AddHours(3);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}