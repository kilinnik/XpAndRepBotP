using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class UserModerationStrategy(
    IUserModerationService userModerationService,
    IChatRepository chatRepository
) : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        var chat = await chatRepository.GetChatByIdAsync(message.Chat.Id, token);

        var user = await userModerationService.GetUserAsync(
            message.From.Id,
            message.Chat.Id,
            token
        );

        var moderationMessage = await userModerationService.CheckAndRemoveWarnIfNeeded(
            user,
            chat.WarnDays,
            token
        );

        return new CommandResult(message.Chat.Id, new List<string> { moderationMessage });
    }
}
