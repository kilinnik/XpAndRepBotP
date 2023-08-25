using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class StatusCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        await using var db = new DbUsersContext();

        if (update.Message.From != null)
        {
            var targetUserId = update.Message.From.Id;
            if (update.Message.ReplyToMessage is { From.IsBot: false } &&
                update.Message.ReplyToMessage.From.Id != 777000)
            {
                targetUserId = update.Message.ReplyToMessage.From.Id;
            }

            var user1 = db.Users.FirstOrDefault(x => x.UserId == targetUserId && x.ChatId == update.Message.Chat.Id);

            if (user1 == null) return;

            string status;
            if (user1.Mariage == 0)
            {
                status = targetUserId == update.Message.From.Id
                    ? "Вы не состоите в браке"
                    : $"{user1.Name} не состоит в браке";
            }
            else
            {
                var user2 = db.Users.First(x => x.UserId == user1.Mariage && x.ChatId == update.Message.Chat.Id);
                var ts = DateTime.Now - user2.DateMariage;
                status =
                    $"🤵🏿 🤵🏿 {user1.Name} состоит в браке с {user2.Name} {ts.Days} d, {ts.Hours} h, {ts.Minutes} m. " +
                    $"Дата регистрации {user1.DateMariage:yy/MM/dd HH:mm:ss}";
            }

            try
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, status, cancellationToken,
                    update.Message.MessageId);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, status, cancellationToken);
            }
        }
    }
}