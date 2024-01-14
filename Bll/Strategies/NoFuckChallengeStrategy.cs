using Bll.Interfaces;
using Domain.DTO;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class NoFuckChallengeStrategy(IUserNfcService userNfcService, ITelegramBotClient botClient)
    : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        var chatMember = await botClient.GetChatMemberAsync(
            message.Chat.Id,
            message.From.Id,
            cancellationToken: token
        );
        
        var user = await userNfcService.GetUserNfcAsync(message.From.Id, message.Chat.Id, token);
        
        var nfcMessages = await userNfcService.EvaluateNfcViolationAsync(
            chatMember.Status,
            user,
            message,
            token
        );
        return new CommandResult(
            message.Chat.Id,
            new List<string> { nfcMessages.Item1, nfcMessages.Item2 }
        );
    }
}
