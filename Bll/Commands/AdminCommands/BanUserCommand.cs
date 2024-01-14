using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bll.Commands.AdminCommands;

public class BanUserCommand(
    ITelegramBotClient botClient,
    IUserRoleService userRoleService,
    IUserModerationService userModerationService,
    ILogger<BanUserCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var executingUser = await userRoleService.GetUserRoleAsync(message.From.Id, message.Chat.Id, token);
            if (!Utils.IsUserModerator(executingUser.Roles))
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, token);
                return null;
            }

            if (message.ReplyToMessage == null || message.ReplyToMessage.From.IsBot)
            {
                return new CommandResult(message.Chat.Id, new List<string> { Resources.ReplyToMsg }, message.MessageId);
            }

            var targetUserId = message.ReplyToMessage.From.Id;
            var targetChatId = message.Chat.Id;

            var chatMember = await botClient.GetChatMemberAsync(targetChatId, targetUserId, token);
            if (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator)
            {
                return new CommandResult(message.Chat.Id, new List<string> { Resources.UserAdmin }, message.MessageId);
            }

            var targetUser = message.ReplyToMessage.From;
            await userModerationService.BanUserAsync(targetUser.Id, message.Chat.Id, token);
            var responseMessage = $"Пользователь {targetUser.FirstName} был забанен.";

            await botClient.DeleteMessageAsync(message.Chat.Id, message.ReplyToMessage.MessageId, token);

            return new CommandResult(message.Chat.Id, new List<string> { responseMessage });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in BanUserCommand");
            return null;
        }
    }
}