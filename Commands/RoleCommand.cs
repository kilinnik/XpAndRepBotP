using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class RoleCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.From == null) return;

        var chatMember =
            await botClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id, cancellationToken);

        var isAuthorizedUser = update.Message.From.Id == Constants.Iid ||
                               (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator &&
                                update.Message.Chat.Id == Constants.NitokinChatId);

        if (!isAuthorizedUser)
        {
            await botClient.DeleteMessageAsync(update.Message, cancellationToken);
            return;
        }

        if (update.Message.ReplyToMessage?.From == null || update.Message.Text == null) return;

        var responseMessage = GiveRoleHandler.GiveRole(update.Message.ReplyToMessage.From.Id, update.Message.Text[6..],
            update.Message.Chat.Id);

        try
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, responseMessage, cancellationToken,
                update.Message.ReplyToMessage.MessageId);
        }
        catch
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, responseMessage, cancellationToken);
        }
    }
}