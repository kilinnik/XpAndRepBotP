using Telegram.Bot.Types;

namespace Bll.Interfaces;

public interface IWelcomeService
{
    Task WelcomeNewMemberAsync(long chatId, User newUser, CancellationToken token);
}