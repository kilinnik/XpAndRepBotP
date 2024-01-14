using Domain.DTO;
using Domain.Models;

namespace Bll.Interfaces;

public interface IUserLevelService
{
    Task<UserLevel> GetUserLevelAsync(long userId, long chatId, CancellationToken token);
    
    Task<IEnumerable<UserLevelDto>> GetTopUsersByLevelAsync(long chatId, int skip, int take, CancellationToken token);
    
    Task<int> GetLevelPositionAsync(long userId, long chatId, CancellationToken token);
    
    Task<string> HandleLevelUpAsync(UserLevel userLevel, CancellationToken token);
}