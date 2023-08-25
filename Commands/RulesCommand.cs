using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class RulesCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await using var db = new DbUsersContext();
        var chat = db.Chats.FirstOrDefault(x => x.ChatId == update.Message.Chat.Id);
        if (chat is not null)
        {
            try
            {
                if (update.Message != null)
                {
                    await botClient.SendTextMessageAsync(chat.ChatId, chat.Rules, cancellationToken,
                        update.Message.MessageId, ParseMode.Html);
                }
            }
            catch
            {
                if (update.Message != null)
                {
                    await botClient.SendTextMessageAsync(chat.ChatId, chat.Rules, cancellationToken,
                        parseMode: ParseMode.Html);
                }
            }
        }
    }
}