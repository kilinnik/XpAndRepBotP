using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserNfcRepository(IDbContextFactory<XpAndRepBotDbContext> contextFactory)
    : IUserNfcRepository
{
    private readonly XpAndRepBotDbContext _context = contextFactory.CreateDbContext();

    public async Task<ChatUser> CreateUserAsync(
        long userId,
        string name,
        string username,
        long chatId,
        CancellationToken token
    )
    {
        var trimmedName = string.IsNullOrEmpty(name) ? "" : name.Trim();

        var safeUsername = string.IsNullOrEmpty(username) ? string.Empty : username;

        var user = new ChatUser(userId, chatId, trimmedName, safeUsername);
        var userReputation = new UserReputation(userId, chatId, trimmedName);

        _context.Users.Add(user);
        _context.UserReputations.Add(userReputation);
        await _context.SaveChangesAsync(token);

        return user;
    }

    public async Task<UserNfc> GetUserAsync(long userId, long chatId, CancellationToken token)
    {
        return await _context
            .UserNfcs.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.ChatId == chatId, token);
    }

    public async Task<List<UserNfc>> GetUsersWithActiveNfcAsync(long chatId, CancellationToken token)
    {
        return await _context
            .UserNfcs.AsNoTracking()
            .Where(u => u.IsNfcActive == true && u.ChatId == chatId)
            .OrderBy(u => u.StartNfcDate)
            .ToListAsync(token);
    }

    public async Task UpdateUserAsync(UserNfc user, CancellationToken token)
    {
        var existingUser = await _context.UserNfcs.FirstOrDefaultAsync(
            u => u.UserId == user.UserId && u.ChatId == user.ChatId,
            token
        );

        if (existingUser != null)
        {
            _context.Entry(existingUser).CurrentValues.SetValues(user);
        }
        else
        {
            _context.UserNfcs.Attach(user);
            _context.Entry(user).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(token);
    }
}
