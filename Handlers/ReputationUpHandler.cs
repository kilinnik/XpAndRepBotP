using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public static class ReputationUpHandler
{
    public static async Task HandleReputationUp(ITelegramBotClient botClient, Update update, DbUsersContext db,
        string messageText, CancellationToken cancellationToken)
    {
        if (update.Message is { ReplyToMessage.From: not null } &&
            (!update.Message.ReplyToMessage.From.IsBot || (update.Message.ReplyToMessage.From.Id == Constants.BotId &&
                                                           update.Message.Chat.Id == Constants.IgruhaChatId)))
        {
            await ReputationUp(botClient, update, db, messageText, cancellationToken);
        }
    }

    private static async Task ReputationUp(ITelegramBotClient botClient, Update update, DbUsersContext db, string mes,
        CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message != null)
            {
                var text = RepUp(update, db, mes);
                if (text != null)
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, text, cancellationToken);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static string RepUp(Update update, DbUsersContext db, string mes)
    {
        mes = Regex.Replace(mes, @"[^\w\d\s+👍👍🏼👍🏽👍🏾👍🏿]", "").ToLower();
        var words = mes.Split(" ");

        if (update.Message?.ReplyToMessage?.From?.Id == update.Message?.From?.Id ||
            !words.Any(Constants.RepWords.Contains))
            return null;

        if (update.Message is not { ReplyToMessage.From: not null }) return null;
        var idUser = update.Message.ReplyToMessage.From.Id;
        var user = db.Users.FirstOrDefault(x => x.UserId == idUser && x.ChatId == update.Message.Chat.Id);
        var user1 = db.Users.FirstOrDefault(x => x.UserId == update.Message.From.Id && x.ChatId == user.ChatId);

        if (user == null || user1 == null) return null;

        user.Rep++;
        db.SaveChanges();

        return $"{user1.Name}({user1.Rep}) увеличил репутацию {user.Name} на 1({user.Rep})";
    }
}