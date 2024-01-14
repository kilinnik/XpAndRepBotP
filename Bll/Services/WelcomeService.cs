using Bll.Interfaces;
using Infrastructure.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Services;

public class WelcomeService(IChatRepository chatRepository, ITelegramBotClient botClient) : IWelcomeService
{
    public async Task WelcomeNewMemberAsync(long chatId, User newUser, CancellationToken token)
    {
        var chat = await chatRepository.GetChatByIdAsync(chatId, token);
        if (chat?.Greeting == null) return;

        var welcomeMessage = $"Привет, {newUser.FirstName}.{chat.Greeting}";
        await botClient.SendTextMessageAsync(chatId, welcomeMessage, cancellationToken: token);
    }
}