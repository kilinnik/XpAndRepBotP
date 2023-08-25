using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class UnRoleAllCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.From == null) return;

        var chatMember =
            await botClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id, cancellationToken);
        var isAuthorizedUser = update.Message.From.Id == Constants.Iid ||
                               (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator &&
                                update.Message.Chat.Id == Constants.NitokinChatId);

        if (isAuthorizedUser && update.Message.ReplyToMessage != null)
        {
            await using var db = new DbUsersContext();
            var user = db.Users.First(x =>
                x.UserId == update.Message.ReplyToMessage.From.Id && x.ChatId == update.Message.Chat.Id);

            var messageText = $"Удалены роли {user.Roles} у {user.Name}";
            try
            {
                await botClient.SendTextMessageAsync(user.ChatId, messageText, cancellationToken,
                    update.Message.MessageId);
            }
            catch
            {
                await botClient.SendTextMessageAsync(user.ChatId, messageText, cancellationToken);
            }

            user.Roles = "";
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await botClient.DeleteMessageAsync(update.Message, cancellationToken);
        }
    }
}