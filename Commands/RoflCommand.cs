using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class RoflCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(x =>
            x.UserId == update.Message.From.Id && x.ChatId == update.Message.Chat.Id);
        if (update.Message != null && user != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"🖕, {user.Name}, иди на хуй 🖕",
                    cancellationToken, update.Message.MessageId);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"🖕, {user.Name}, иди на хуй 🖕",
                    cancellationToken);
            }
        }
    }
}