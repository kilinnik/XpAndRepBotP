using Domain.DTO;
using Domain.Models;
using Telegram.Bot.Types;

namespace Bll.Interfaces;

public interface IUserReputationService
{
    Task<UserReputation> GetUserAsync(long userId, long chatId, CancellationToken token);
    
    Task<string> HandleReputationUpAsync(Message message, CancellationToken token);
    
    Task<int> GetReputationPositionAsync(long userId, long chatId, CancellationToken token);
    
    Task<IEnumerable<UserReputationDto>> GetTopUsersByReputationAsync(long chatId, int skip, int take,
        CancellationToken token);
}