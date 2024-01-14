using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserModerationRepository(IDbContextFactory<XpAndRepBotDbContext> contextFactory)
    : IUserModerationRepository
{
    public async Task<UserModeration> GetUserAsync(long userId, long chatId, CancellationToken token)
    {
        await using var context = await contextFactory.CreateDbContextAsync(token);

        return await context
            .UserModerations.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.ChatId == chatId, token);
    }

    public async Task UpdateUserAsync(UserModeration user, CancellationToken token)
    {
        await using var context = await contextFactory.CreateDbContextAsync(token);
        
        context.UserModerations.Attach(user);
        context.Entry(user).State = EntityState.Modified;

        await context.SaveChangesAsync(token);
    }
}