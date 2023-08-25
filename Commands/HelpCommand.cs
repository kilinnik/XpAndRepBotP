using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class HelpCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await using var db = new DbUsersContext();
        var chat = db.Chats.FirstOrDefault(x => x.ChatId == update.Message.Chat.Id);
        if (chat is not null && update.Message != null)
        {
            try
            {
                await botClient.SendTextMessageAsync(chat.ChatId, chat.HelpCommands, cancellationToken,
                    update.Message.MessageId, ParseMode.Html);
            }
            catch
            {
                await botClient.SendTextMessageAsync(chat.ChatId, chat.HelpCommands, cancellationToken,
                    parseMode: ParseMode.Html);
            }
        }
    }
}