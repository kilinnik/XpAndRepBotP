using Bll.Interfaces;
using Domain.DTO;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class UserRoleMentionsStrategy(IUserRoleService userRoleService) : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        return await userRoleService.HandleMentionsAsync(message, token);
    }
}