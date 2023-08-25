using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class ReportCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        string responseMessage;

        await using var db = new DbUsersContext();
        var user1 = db.Users.FirstOrDefault(x =>
            x.UserId == update.Message.ReplyToMessage.From.Id && x.ChatId == update.Message.Chat.Id);
        var user2 = db.Users.FirstOrDefault(x =>
            x.UserId == update.Message.From.Id && x.ChatId == update.Message.Chat.Id);

        if (update.Message.Text != null && update.Message.ReplyToMessage != null && update.Message.Text.Length > 7)
        {
            if (user1 == null || user2 == null) return;

            if (!user1.Complainers.Contains(user2.UserId.ToString()))
            {
                user1.Complainers += user2.UserId + ",";
                user1.Complaints += $"{update.Message.Text[8..]} ({user2.Name})\n";
                await db.SaveChangesAsync(cancellationToken);
                responseMessage = "Ваша жалоба принята";
            }
            else
            {
                responseMessage = "Ваша жалоба отклонена, вы уже жаловались на этого пользователя";
            }
        }
        else
        {
            responseMessage = "Ваша жалоба отклонена, вы не ответили на сообщение пользователя или не написали жалобу";
        }

        try
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, responseMessage, cancellationToken,
                update.Message.MessageId);
        }
        catch
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, responseMessage, cancellationToken);
        }
    }
}