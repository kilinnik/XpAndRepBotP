using Domain.DTO;
using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IUserReputationRepository
{
    Task<UserReputation> GetUserReputationAsync(long userId, long chatId, CancellationToken token);
    
    Task<int> GetReputationPositionAsync(long userId, long chatId, CancellationToken token);

    Task<IEnumerable<UserReputationDto>> GetTopUsersByReputationAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    );
    
    Task UpdateUserAsync(UserReputation user, CancellationToken token);
}
