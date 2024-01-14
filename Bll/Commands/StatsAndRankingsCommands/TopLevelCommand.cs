using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Commands.StatsAndRankingsCommands;

public class TopLevelCommand(IUserLevelService userLevelService, ILogger<TopLevelCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var users = await userLevelService.GetTopUsersByLevelAsync(message.Chat.Id, 0, 50, token);

            var response = Utils.FormatTopLevelUsers(users, 0);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "backtl|0"),
                InlineKeyboardButton.WithCallbackData("Вперёд", "nexttl|0")
            });

            return new CommandResult(message.Chat.Id, new List<string> { response }, message.MessageId,
                new List<InlineKeyboardMarkup> { inlineKeyboard });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in TopLevelCommand");
            return null;
        }
    }
}