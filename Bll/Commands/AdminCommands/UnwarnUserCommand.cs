using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Commands.AdminCommands;

public class UnwarnUserCommand(
    ITelegramBotClient botClient,
    IUserRoleService userRoleService,
    IUserModerationService userModerationService,
    ILogger<UnwarnUserCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var executingUser = await userRoleService.GetUserRoleAsync(message.From.Id, message.Chat.Id, token);
            if (!IsUserModerator(executingUser.Roles))
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, token);
                return null;
            }

            if (message.ReplyToMessage == null || message.ReplyToMessage.From.IsBot)
            {
                return new CommandResult(message.Chat.Id, new List<string> { Resources.ReplyToMsg }, message.MessageId);
            }

            var unwarnedUser =
                await userModerationService.GetUserAsync(message.ReplyToMessage.From.Id, message.Chat.Id, token);

            if (unwarnedUser == null)
            {
                return new CommandResult(message.Chat.Id, new List<string> { "Пользователь не найден" },
                    message.MessageId);
            }

            var unwarnMessage = await userModerationService.UnwarnUserAsync(unwarnedUser, token);

            return new CommandResult(message.Chat.Id, new List<string> { unwarnMessage },
                message.ReplyToMessage.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in UnwarnUserCommand");
            return null;
        }
    }

    private static bool IsUserModerator(string userRoles)
    {
        return userRoles.Contains("модер");
    }
}