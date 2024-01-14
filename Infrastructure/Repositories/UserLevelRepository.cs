using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserLevelRepository(IDbContextFactory<XpAndRepBotDbContext> contextFactory) : IUserLevelRepository
{
    public async Task<UserLevel> GetUserLevelAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(token);

        return await context
            .UserLevels.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.ChatId == chatId, token);
    }

    public async Task<int> GetLevelPositionAsync(long userId, long chatId, CancellationToken token)
    {
        await using var context = await contextFactory.CreateDbContextAsync(token);

        var user = await context
            .UserLevels.AsNoTracking()
            .Where(u => u.UserId == userId && u.ChatId == chatId)
            .Select(u => new { u.Level, u.CurrentExperience })
            .FirstOrDefaultAsync(token);

        if (user == null)
            return -1;

        var position = await context
            .UserLevels.AsNoTracking()
            .Where(
                u =>
                    u.ChatId == chatId
                    && (
                        u.Level > user.Level
                        || (u.Level == user.Level && u.CurrentExperience > user.CurrentExperience)
                    )
            )
            .CountAsync(token);

        return position + 1;
    }

    public async Task<IEnumerable<UserLevelDto>> GetTopUsersByLevelAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(token);

        var users = await context
            .UserLevels.AsNoTracking()
            .Where(x => x.ChatId == chatId)
            .OrderByDescending(x => x.Level)
            .ThenByDescending(x => x.CurrentExperience)
            .Skip(skip)
            .Take(take)
            .Select(
                u =>
                    new UserLevelDto(
                        u.FirstName,
                        u.Level,
                        u.CurrentExperience,
                        Utils.XpForLvlUp[u.Level]
                    )
            )
            .ToListAsync(token);

        return users;
    }

    public async Task UpdateUserLevelAsync(UserLevel userLevel, CancellationToken token)
    {
        await using var context = await contextFactory.CreateDbContextAsync(token);
        
        var existingUser = context.UserReputations.Local.FirstOrDefault(
            u => u.UserId == userLevel.UserId && u.ChatId == userLevel.ChatId
        );

        if (existingUser != null)
        {
            context.Entry(existingUser).CurrentValues.SetValues(userLevel);
        }
        else
        {
            context.UserLevels.Update(userLevel);
        }

        await context.SaveChangesAsync(token);
    }
}