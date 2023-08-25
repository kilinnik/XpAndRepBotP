using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;
using XpAndRepBot.Handlers;

namespace XpAndRepBot.Commands;

public class UnwarnCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.From == null) return;

        var chatMember =
            await botClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id, cancellationToken);
        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(x => x.UserId == update.Message.From.Id && x.Roles.Contains("модер"));

        var isAuthorizedUser = update.Message.ReplyToMessage?.From != null &&
                               update.Message.From.Id != update.Message.ReplyToMessage?.From.Id &&
                               (user != null ||
                                (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator &&
                                 update.Message.Chat.Id == Constants.NitokinChatId));

        switch (isAuthorizedUser)
        {
            case true when update.Message.ReplyToMessage?.From != null:
            {
                var messageText = UpdateWarnHandler.UpdateWarn(update, false);
                try
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, messageText, cancellationToken,
                        update.Message.ReplyToMessage.MessageId);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, messageText, cancellationToken);
                }

                break;
            }
            case false:
                try
                {
                    await botClient.DeleteMessageAsync(update.Message, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                break;
        }
    }
}