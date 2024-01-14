using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.RelationshipCommands;

public class DivorceCommand(IUserMarriageService userMarriageService, ILogger<DivorceCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var responseMessage = await userMarriageService.ProcessDivorceAsync(message.From.Id, message.Chat.Id, token);

            return new CommandResult(message.Chat.Id, new List<string> { responseMessage }, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in DivorceCommand");
            return null;
        }
    }
}