using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.RelationshipCommands;

public class MarriageListCommand(IUserMarriageService userMarriageService, ILogger<MarriageListCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var marriagesInfo = await userMarriageService.GetMarriagesAsync(message.Chat.Id, token);

            return new CommandResult(message.Chat.Id, new List<string> { marriagesInfo }, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in MarriageListCommand");
            return null;
        }
    }
}