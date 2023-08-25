using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Commands;

public class DivorceCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        await using var db = new DbUsersContext();
        var user1 = db.Users.FirstOrDefault(x =>
            x.UserId == update.Message.From.Id && x.ChatId == update.Message.Chat.Id);

        if (user1 == null || user1.Mariage == 0)
        {
            await InformUserNotInMarriage(botClient, user1, cancellationToken);
            return;
        }

        var user2 = db.Users.FirstOrDefault(x => x.UserId == user1.Mariage && x.ChatId == user1.ChatId);

        if (user2 == null) return;

        await ProcessDivorce(user1, user2, botClient, update, db, cancellationToken);
    }

    private static async Task InformUserNotInMarriage(ITelegramBotClient botClient, Users user,
        CancellationToken cancellationToken)
    {
        if (user != null)
        {
            await botClient.SendTextMessageAsync(user.ChatId, "Вы не состоите в браке", cancellationToken);
        }
    }

    private static async Task ProcessDivorce(Users user1, Users user2, ITelegramBotClient botClient, Update update,
        DbUsersContext db, CancellationToken cancellationToken)
    {
        user1.Mariage = 0;
        user2.Mariage = 0;

        var ts = DateTime.Now - user2.DateMariage;

        await db.SaveChangesAsync(cancellationToken);

        var message = $"💔 {user2.Name} сожалеем, но {user1.Name} подал на развод. Ваш брак был зарегистрирован " +
                      $"{user1.DateMariage:yy/MM/dd HH:mm:ss} и просуществовал {ts.Days} d, {ts.Hours} h, {ts.Minutes} m";

        if (update.Message != null)
        {
            await botClient.SendTextMessageAsync(user1.ChatId, message, cancellationToken, update.Message.MessageId);
        }
    }
}