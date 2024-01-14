using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Commands.AdminCommands;

public class AssignRoleCommand(
    ITelegramBotClient botClient,
    IUserRoleService userRoleService,
    ILogger<AssignRoleCommand> logger
) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var executingUser = await userRoleService.GetUserRoleAsync(
                message.From.Id,
                message.Chat.Id,
                token
            );

            if (!Utils.IsUserModerator(executingUser.Roles))
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, token);
                return null;
            }

            if (message.ReplyToMessage == null || message.ReplyToMessage.From.IsBot)
            {
                return new CommandResult(
                    message.Chat.Id,
                    new List<string> { Resources.ReplyToMsg },
                    message.MessageId
                );
            }

            var messageParts = message.Text?.Split(' ');

            if (messageParts is { Length: <= 1 })
            {
                const string messageText = "Пожалуйста, укажите роль после команды /role.";
                return new CommandResult(
                    message.Chat.Id,
                    new List<string> { messageText },
                    message.MessageId
                );
            }

            var newRole = string.Join(" ", messageParts.Skip(1)).Trim();
            var responseMessage = await userRoleService.UpdateUserRoleAsync(
                message.ReplyToMessage.From.Id,
                message.Chat.Id,
                message.ReplyToMessage.From.FirstName,
                newRole,
                token
            );

            return new CommandResult(
                message.Chat.Id,
                new List<string> { responseMessage },
                message.ReplyToMessage.MessageId
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in AssignRoleCommand");
            return null;
        }
    }
}
