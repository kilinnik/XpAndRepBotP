using Bll.Interfaces;
using Domain.DTO;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class UserLevelStrategy(IUserLevelService userLevelService) : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        var user = await userLevelService.GetUserLevelAsync(
            message.From.Id,
            message.Chat.Id,
            token
        );
        var messageText = message.Text ?? message.Caption ?? string.Empty;
        try
        {
            user.CurrentExperience += messageText.Length;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(user.CurrentExperience + @" " + messageText.Length);
        }

        var levelUpMessage = await userLevelService.HandleLevelUpAsync(user, token);
        return new CommandResult(message.Chat.Id, new List<string> { levelUpMessage });
    }
}