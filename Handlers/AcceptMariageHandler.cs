using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public static class AcceptMariageHandler
{
    public static async Task<InlineKeyboardMarkup> AcceptMariage(string option, CallbackQuery callbackQuery,
        ITelegramBotClient botClient, long chatId, long userId, CancellationToken cancellationToken)
    {
        await using var db = new DbUsersContext();
        if (callbackQuery.Message == null) return null;
        var inlineKeyboard = callbackQuery.Message.ReplyMarkup;
        var user2 = db.Users.First(x => x.UserId == userId && x.ChatId == chatId);
        if (callbackQuery.From.Id != userId || user2.Mariage != 0) return inlineKeyboard;

        var match = Regex.Match(option, @"\d+");
        var id = long.Parse(match.Value);
        var user1 = db.Users.First(x => x.UserId == id && x.ChatId == chatId);

        string text;
        if (option[1] == 'y')
        {
            user1.Mariage = user2.UserId;
            user2.Mariage = user1.UserId;
            user1.DateMariage = DateTime.Now;
            user2.DateMariage = user1.DateMariage;
            await db.SaveChangesAsync(cancellationToken);
            text = $"👰🏿 👰🏿 {user2.Name} и {user1.Name} заключили брак";
        }
        else
        {
            text = $"{user2.Name} отказался от брака c {user1.Name}";
        }

        await botClient.SendTextMessageAsync(chatId, text, cancellationToken);
        return null;
    }
}