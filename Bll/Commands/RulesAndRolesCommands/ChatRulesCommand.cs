using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.RulesAndRolesCommands;

public class ChatRulesCommand(IChatRepository chatRepository, ILogger<ChatRulesCommand> logger)
    : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var chat = await chatRepository.GetChatByIdAsync(message.Chat.Id, token);

            return new CommandResult(
                message.Chat.Id,
                new List<string> { chat.Rules },
                message.MessageId
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in ChatRulesCommand");
            return null;
        }
    }
}
