using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IUserModerationRepository
{
    Task<UserModeration> GetUserAsync(long userId, long chatId, CancellationToken token);
    
    Task UpdateUserAsync(UserModeration user, CancellationToken token);
}