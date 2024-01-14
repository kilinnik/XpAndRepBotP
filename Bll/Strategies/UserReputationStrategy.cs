using Bll.Interfaces;
using Domain.DTO;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class UserReputationStrategy(IUserReputationService userReputationService) : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        var reputationMessage = await userReputationService.HandleReputationUpAsync(message, token);
        return new CommandResult(message.Chat.Id, new List<string> { reputationMessage });
    }
}