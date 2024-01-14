using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository(IDbContextFactory<XpAndRepBotDbContext> contextFactory)
    : IUserRepository
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
        var userLevel = new UserLevel(userId, chatId, trimmedName);
        var userModeration = new UserModeration(userId, chatId, trimmedName);

        _context.Users.Add(user);
        _context.UserReputations.Add(userReputation);
        _context.UserLevels.Add(userLevel);
        _context.UserModerations.Add(userModeration);
        
        await _context.SaveChangesAsync(token);

        return user;
    }

    public async Task<ChatUser?> GetUserAsync(long userId, long chatId, CancellationToken token)
    {
        return await _context
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.ChatId == chatId, token);
    }
    
    public async Task<long?> GetUserIdByUsernameAsync(
        string username,
        long chatId,
        CancellationToken token
    )
    {
        var user = await _context
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username && u.ChatId == chatId, token);

        return user?.UserId;
    }

    public async Task UpdateUserAsync(ChatUser chatUser, CancellationToken token)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(
            u => u.UserId == chatUser.UserId && u.ChatId == chatUser.ChatId,
            token
        );

        if (existingUser != null)
        {
            _context.Entry(existingUser).CurrentValues.SetValues(chatUser);
        }
        else
        {
            _context.Users.Attach(chatUser);
            _context.Entry(chatUser).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(token);
    }
}
