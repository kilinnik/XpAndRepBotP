using Domain.DTO;
using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IUserLevelRepository
{
    Task<UserLevel> GetUserLevelAsync(long userId, long chatId, CancellationToken token);
    
    Task<int> GetLevelPositionAsync(long userId, long chatId, CancellationToken token);

    Task<IEnumerable<UserLevelDto>> GetTopUsersByLevelAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    );
    
    Task UpdateUserLevelAsync(UserLevel userLevel, CancellationToken token);
}
