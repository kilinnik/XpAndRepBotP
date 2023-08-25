using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class UnBanCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.From == null) return;

        var chatMember =
            await botClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id, cancellationToken);
        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(x => x.UserId == update.Message.From.Id && x.Roles.Contains("модер"));

        var isModeratorOrAdmin = user != null ||
                                 (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator &&
                                  update.Message.Chat.Id == Constants.NitokinChatId);

        if (update.Message.ReplyToMessage is { From: not null } &&
            update.Message.From.Id != update.Message.ReplyToMessage.From.Id && isModeratorOrAdmin)
        {
            long userId = 0;
            var name = "";

            if (update.Message.Entities?.Any(x => x.Type == MessageEntityType.Mention) == true)
            {
                foreach (var entity in update.Message.Entities.Where(e => e.Type == MessageEntityType.Mention))
                {
                    var userMention = db.Users.FirstOrDefault(x =>
                        x.Username == update.Message.Text.Substring(entity.Offset + 1, entity.Length - 1));
                    if (userMention == null) continue;
                    userId = userMention.UserId;
                    name = userMention.Name;
                    break;
                }
            }
            else
            {
                userId = update.Message.ReplyToMessage.From.Id;
                name = update.Message.ReplyToMessage.From.FirstName;
            }

            try
            {
                await botClient.RestrictChatMemberAsync(
                    chatId: update.Message.Chat.Id,
                    userId,
                    new ChatPermissions
                    {
                        CanSendMessages = true,
                        CanSendMediaMessages = true,
                        CanSendOtherMessages = true,
                        CanSendPolls = true,
                        CanAddWebPagePreviews = true,
                        CanChangeInfo = true,
                        CanInviteUsers = true,
                        CanPinMessages = true,
                    },
                    cancellationToken: cancellationToken);

                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{name} разбанен", cancellationToken,
                    update.Message.MessageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        else
        {
            try
            {
                await botClient.DeleteMessageAsync(update.Message, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}