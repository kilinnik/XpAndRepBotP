using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IUserNfcRepository
{
    // Task<User> CreateUserAsync(
    //     long userId,
    //     string name,
    //     string username,
    //     long chatId,
    //     CancellationToken token
    // );
    //
    Task<UserNfc> GetUserAsync(long userId, long chatId, CancellationToken token);
    
    Task<List<UserNfc>> GetUsersWithActiveNfcAsync(long chatId, CancellationToken token);
    
    Task UpdateUserAsync(UserNfc user, CancellationToken token);
}
