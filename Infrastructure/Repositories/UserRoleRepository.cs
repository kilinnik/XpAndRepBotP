using Domain.DTO;
using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRoleRepository(IDbContextFactory<XpAndRepBotDbContext> contextFactory)
    : IUserRoleRepository
{
    private readonly XpAndRepBotDbContext _context = contextFactory.CreateDbContext();

    public async Task<UserRole?> GetUserRoleAsync(long userId, long chatId, CancellationToken token)
    {
        return await _context
            .UserRoles.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.ChatId == chatId, token);
    }

    public async Task<IEnumerable<RoleDto>> GetRolesListAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        var usersWithRoles = await _context
            .UserRoles.AsNoTracking()
            .Where(u => u.ChatId == chatId && !string.IsNullOrEmpty(u.Roles))
            .Select(u => new { Name = u.FirstName, u.Roles })
            .ToListAsync(token);

        var allRoles = usersWithRoles
            .SelectMany(u => u.Roles.Split(", ", StringSplitOptions.RemoveEmptyEntries))
            .Distinct()
            .OrderBy(role => role)
            .ToList();

        var paginatedRoles = allRoles.Skip(skip).Take(take).ToList();

        return (
            from role in paginatedRoles
            let usersInRole = usersWithRoles
                .Where(
                    u => u.Roles.Split(", ", StringSplitOptions.RemoveEmptyEntries).Contains(role)
                )
                .Select(u => u.Name)
                .ToList()
            select new RoleDto(role, usersInRole)
        ).ToList();
    }

    public async Task<List<UserRole>> GetUsersByRoleAsync(
        long chatId,
        string role,
        CancellationToken token
    )
    {
        return await _context
            .UserRoles.AsNoTracking()
            .Where(u => u.ChatId == chatId && u.Roles.Contains(role))
            .ToListAsync(token);
    }

    public async Task<string> UpdateUserRoleAsync(
        long userId,
        long chatId,
        string firstName,
        string role,
        CancellationToken token
    )
    {
        var user = await _context.UserRoles.FirstOrDefaultAsync(
            u => u.UserId == userId && u.ChatId == chatId,
            token
        );

        if (user == null)
        {
            user = new UserRole(userId, chatId, firstName, role);
            _context.UserRoles.Add(user);
            await _context.SaveChangesAsync(token);
            return $"{firstName} создан с ролью {role}.";
        }

        if (SplitRoles(user.Roles).Contains(role))
        {
            return $"{user.FirstName} уже имеет роль {role}";
        }

        UpdateRoles(user, role);
        await _context.SaveChangesAsync(token);
        return $"{user.FirstName} получает роль {role}";
    }

    public async Task<string> RemoveAllRolesAsync(long userId, long chatId, CancellationToken token)
    {
        var user = await _context.UserRoles.FirstOrDefaultAsync(
            u => u.UserId == userId && u.ChatId == chatId,
            token
        );

        if (user == null)
        {
            return "Пользователь не найден.";
        }

        _context.UserRoles.Remove(user);
        await _context.SaveChangesAsync(token);

        return $"У пользователя {user.FirstName} удалены все роли.";
    }

    public async Task<string> RemoveRoleAsync(
        long userId,
        long chatId,
        string role,
        CancellationToken token
    )
    {
        var user = await _context.UserRoles.FirstOrDefaultAsync(
            u => u.UserId == userId && u.ChatId == chatId,
            token
        );

        if (user == null)
        {
            return "Пользователь не найден.";
        }

        var roles = new HashSet<string>(SplitRoles(user.Roles));

        if (roles.Remove(role))
        {
            if (roles.Count == 0)
            {
                _context.UserRoles.Remove(user);
            }
            else
            {
                user.Roles = string.Join(", ", roles);
            }
            await _context.SaveChangesAsync(token);
            return $"Роль {role} удалена у пользователя {user.FirstName}.";
        }

        return $"У пользователя {user.FirstName} нет роли {role}.";
    }

    private static void UpdateRoles(UserRole user, string newRole)
    {
        var roles = SplitRoles(user.Roles).ToList();
        if (roles.Contains(newRole))
        {
            return;
        }

        roles.Add(newRole);
        user.Roles = string.Join(", ", roles);
    }

    private static string[] SplitRoles(string roles)
    {
        return roles.Split(", ", StringSplitOptions.RemoveEmptyEntries);
    }
}
