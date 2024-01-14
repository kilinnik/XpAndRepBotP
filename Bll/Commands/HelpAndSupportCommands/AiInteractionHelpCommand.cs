using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.HelpAndSupportCommands;

public class AiInteractionHelpCommand(ILogger<AiInteractionHelpCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            return new CommandResult(message.Chat.Id, new List<string> { Resources.HelpAiText }, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in AiInter actionHelpCommand");
            return null;
        }
    }
}