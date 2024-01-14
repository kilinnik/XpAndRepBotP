using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class AiResponseStrategy(IAiResponseService aiResponseService, ITelegramBotClient botClient) : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        await aiResponseService.HandleMessageAsync(botClient, message, token);
        return null; 
    }
}