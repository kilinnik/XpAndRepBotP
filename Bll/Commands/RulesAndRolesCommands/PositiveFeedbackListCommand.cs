using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.RulesAndRolesCommands;

public class PositiveFeedbackListCommand(ILogger<PositiveFeedbackListCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            return new CommandResult(message.Chat.Id, new List<string> { Resources.PositiveFeedbackPhrases },
                message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in ReputationMessagesListCommand");
            return null;
        }
    }
}