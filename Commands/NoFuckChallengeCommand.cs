using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class NoFuckChallengeCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(
            x => x.UserId == update.Message.From.Id && x.ChatId == update.Message.Chat.Id);

        if (user == null) return;

        string responseMessage;

        if (user.Nfc != null && (bool)user.Nfc)
        {
            responseMessage = PrintNfcHandler.PrintNfc(user.ChatId);
        }
        else
        {
            user.Nfc = true;
            var bestTime = "";

            if (user.BestTime > 0)
            {
                var ts = TimeSpan.FromTicks(user.BestTime);
                bestTime = $"\nВаш лучший результат: {ts.Days} d, {ts.Hours} h, {ts.Minutes} m.";
            }

            user.StartNfc = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);

            responseMessage = $"Вы начали новую серию без мата 👮‍♂️{bestTime}\nУдачи 😉";
        }

        try
        {
            await botClient.SendTextMessageAsync(user.ChatId, responseMessage, cancellationToken,
                update.Message.MessageId);
        }
        catch
        {
            await botClient.SendTextMessageAsync(user.ChatId, responseMessage, cancellationToken);
        }
    }
}