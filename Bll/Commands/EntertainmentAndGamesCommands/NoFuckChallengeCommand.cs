using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.EntertainmentAndGamesCommands;

public class NoFuckChallengeCommand(
    IUserNfcService userNfcService,
    ILogger<NoFuckChallengeCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken cancellationToken)
    {
        try
        {
            var responseMessage = await userNfcService.StartOrCheckNfcAsync(message.From.Id, message.Chat.Id, cancellationToken);

            return new CommandResult(message.Chat.Id, new List<string> { responseMessage }, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in NoFuckChallengeCommand");
            return null;
        }
    }
}