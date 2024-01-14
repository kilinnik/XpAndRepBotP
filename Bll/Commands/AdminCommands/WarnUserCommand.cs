using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Commands.AdminCommands;

public class WarnUserCommand(
    ITelegramBotClient botClient,
    IUserRepository userRepository,
    IUserRoleService userRoleService,
    IUserModerationService userModerationService,
    ILogger<WarnUserCommand> logger) : ICommand
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

            var warnedUser = await userModerationService.GetUserAsync(message.ReplyToMessage.From.Id, message.Chat.Id, token);
            if (warnedUser == null)
            {
                return new CommandResult(message.Chat.Id, new List<string> { "Пользователь не найден" },
                    message.MessageId);
            }

            var warnMessage =await userModerationService.WarnUserAsync(warnedUser, token);

            return new CommandResult(message.Chat.Id, new List<string> { warnMessage },
                message.ReplyToMessage.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in WarnUserCommand");
            return null;
        }
    }
}