using Domain.DTO;
using Domain.Models;
using Telegram.Bot.Types;

namespace Bll.Interfaces;

public interface IUserRoleService
{
    Task<UserRole?> GetUserRoleAsync(long userId, long chatId, CancellationToken token);

    Task<IEnumerable<RoleDto>> GetRolesListAsync(long chatId, int skip, int take, CancellationToken token);
    
    Task<string> UpdateUserRoleAsync(long userId, long chatId, string firstName, string role, CancellationToken token);
    
    Task<string> RemoveAllRolesAsync(long userId, long chatId, CancellationToken token);
    
    Task<string> RemoveRoleAsync(long userId, long chatId, string role, CancellationToken token);
    
    Task<CommandResult> HandleMentionsAsync(Message message, CancellationToken token);
}