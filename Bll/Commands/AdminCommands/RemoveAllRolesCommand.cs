using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Commands.AdminCommands;

public class RemoveAllRolesCommand(
    ITelegramBotClient botClient,
    IUserRoleService userRoleService,
    ILogger<RemoveAllRolesCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message,  CancellationToken token)
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

            var responseMessage =
                await userRoleService.RemoveAllRolesAsync(message.ReplyToMessage.From.Id, message.Chat.Id, token);

            return new CommandResult(message.Chat.Id, new List<string> { responseMessage }, message.ReplyToMessage.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in RemoveAllRolesCommand");
            return null;
        }
    }
}