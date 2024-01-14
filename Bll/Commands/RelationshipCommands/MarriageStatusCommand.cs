using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.RelationshipCommands;

public class MarriageStatusCommand(IUserMarriageService userMarriageService, ILogger<MarriageStatusCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var targetUserId = message.From?.Id ?? 0;
            if (message.ReplyToMessage != null && !message.ReplyToMessage.From.IsBot)
            {
                targetUserId = message.ReplyToMessage.From.Id;
            }

            var status = await userMarriageService.GetMarriageStatusAsync(targetUserId, message.Chat.Id, token);

            return new CommandResult(message.Chat.Id, new List<string> { status }, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in MarriageStatusCommand");
            return null;
        }
    }
}