using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class UnRoleCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.From == null) return;

        var chatMember =
            await botClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id, cancellationToken);
        var isAuthorizedUser = update.Message.From.Id == Constants.Iid ||
                               (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator &&
                                update.Message.Chat.Id == Constants.NitokinChatId);

        switch (isAuthorizedUser)
        {
            case true when update.Message.ReplyToMessage?.From != null && update.Message.Text != null:
            {
                var messageText = DelRoleHandler.DelRole(update.Message.ReplyToMessage.From.Id,
                    update.Message.Text[5..], update.Message.Chat.Id);
                try
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, messageText, cancellationToken,
                        update.Message.MessageId);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, messageText, cancellationToken);
                }

                break;
            }
            case false when update.Message != null:
                await botClient.DeleteMessageAsync(update.Message, cancellationToken);
                break;
        }
    }
}