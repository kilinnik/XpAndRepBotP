using Bll.Interfaces;
using Domain.DTO;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class UserLexiconStrategy(IUserLexiconService userLexiconService) : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        await userLexiconService.UpdateWordUsageAsync(message.From.Id, message.Chat.Id, message.Text ?? message.Caption ?? string.Empty, token);
        return null;
    }
}