using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bll.Commands.StatsAndRankingsCommands;

public class TopReputationCommand(IUserReputationService userReputationService, ILogger<TopReputationCommand> logger)
    : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var users = await userReputationService.GetTopUsersByReputationAsync(message.Chat.Id, 0, 50, token);

            var response = Utils.FormatTopUsersByReputation(users, 0);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "backtr|0"),
                InlineKeyboardButton.WithCallbackData("Вперёд", "nexttr|0")
            });

            return new CommandResult(message.Chat.Id, new List<string> { response }, message.MessageId,
                new List<InlineKeyboardMarkup> { inlineKeyboard });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in TopReputationCommand");
            return null;
        }
    }
}