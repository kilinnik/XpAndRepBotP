using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IUserRepository
{
    Task<ChatUser> CreateUserAsync(
        long userId,
        string name,
        string username,
        long chatId,
        CancellationToken token
    );

    Task<ChatUser?> GetUserAsync(long userId, long chatId, CancellationToken token);

    Task<long?> GetUserIdByUsernameAsync(string username, long chatId, CancellationToken token);

    Task UpdateUserAsync(ChatUser chatUser, CancellationToken token);
}
