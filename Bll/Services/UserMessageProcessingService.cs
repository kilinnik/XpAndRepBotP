using Bll.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Services;

public class UserMessageProcessingService(
    IUserRepository userRepository,
    ITelegramMessageService messageService,
    ILogger<UserMessageProcessingService> logger,
    IEnumerable<IUserMessageStrategy> messageStrategies
) : IUserMessageProcessingService
{
    public async Task ProcessUserMessageAsync(Message message, ChatUser chatUser, CancellationToken token)
    {
        try
        {
            var tasks = messageStrategies.Select(
                strategy => strategy.ExecuteAsync(message, token)
            );
            var commandResults = (await Task.WhenAll(tasks)).Where(result => result != null);

            await userRepository.UpdateUserAsync(chatUser, token);

            foreach (var result in commandResults)
            {
                if (result.Texts?.Count > 0)
                {
                    await messageService.SendMessageAsync(result, token);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while processing user message");
        }
    }
}
