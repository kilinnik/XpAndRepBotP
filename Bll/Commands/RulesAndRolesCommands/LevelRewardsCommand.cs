using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.RulesAndRolesCommands;

public class LevelRewardsCommand(ILogger<LevelRewardsCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            return new CommandResult(message.Chat.Id, new List<string> { Resources.LevelRewards }, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in LevelRewardsCommand");
            return null;
        }
    }
}