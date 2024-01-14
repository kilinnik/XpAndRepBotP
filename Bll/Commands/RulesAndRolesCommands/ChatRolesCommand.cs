using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Commands.RulesAndRolesCommands;

public class ChatRolesCommand(IUserRoleService userRoleService, ILogger<ChatRolesCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var roles = await userRoleService.GetRolesListAsync(message.Chat.Id, 0, 20, token);

            if (!roles.Any())
            {
                return new CommandResult(message.Chat.Id, new List<string> { "Нет ролей" }, message.MessageId);
            }

            var formattedRoles = Utils.FormatRoles(roles, 0);
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "backr|0"),
                InlineKeyboardButton.WithCallbackData("Вперёд", "nextr|0")
            });

            return new CommandResult(message.Chat.Id, new List<string> { formattedRoles }, message.MessageId,
                new List<InlineKeyboardMarkup> { inlineKeyboard });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in ChatRolesCommand");
            return null;
        }
    }
}