using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Commands.AdminCommands;

public class RemoveRoleCommand(
    ITelegramBotClient botClient,
    IUserRoleService userRoleService,
    ILogger<RemoveRoleCommand> logger) : ICommand
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

            var text = message.Text ?? message.Caption ?? string.Empty;
            var commandLength = !text.Contains(' ') ? text.Length : text.IndexOf(' ');
            var role = text[commandLength..].Trim();

            if (string.IsNullOrEmpty(role))
            {
                return new CommandResult(message.Chat.Id, new List<string> { "Укажите роль" }, message.MessageId);
            }

            var responseText = await userRoleService.RemoveRoleAsync(message.ReplyToMessage.From.Id, message.Chat.Id,
                role,
                token);

            return new CommandResult(message.Chat.Id, new List<string> { responseText },
                message.ReplyToMessage.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in RemoveRoleCommand");
            return null;
        }
    }
}