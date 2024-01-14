using Domain.DTO;
using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IUserRoleRepository
{
    Task<UserRole?> GetUserRoleAsync(long userId, long chatId, CancellationToken token);

    Task<IEnumerable<RoleDto>> GetRolesListAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    );

    Task<List<UserRole>> GetUsersByRoleAsync(long chatId, string role, CancellationToken token);

    Task<string> UpdateUserRoleAsync(
        long userId,
        long chatId,
        string firstName,
        string role,
        CancellationToken token
    );

    Task<string> RemoveAllRolesAsync(long userId, long chatId, CancellationToken token);

    Task<string> RemoveRoleAsync(long userId, long chatId, string role, CancellationToken token);
}
