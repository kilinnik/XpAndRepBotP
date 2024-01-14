using Domain.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bll.Interfaces;

public interface IUserNfcService
{
    Task<UserNfc> GetUserNfcAsync(long userId, long chatId, CancellationToken token);

    Task<string> StartOrCheckNfcAsync(long userId, long chatId, CancellationToken token);

    Task<(string, string)> EvaluateNfcViolationAsync(ChatMemberStatus status, UserNfc userNfc, Message message,
        CancellationToken token);
}