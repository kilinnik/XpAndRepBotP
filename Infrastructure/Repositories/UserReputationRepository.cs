using Domain.DTO;
using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserReputationRepository(XpAndRepBotDbContext context) : IUserReputationRepository
{
    public async Task<UserReputation> GetUserReputationAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        return await context
            .UserReputations.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.ChatId == chatId, token);
    }

    public async Task<int> GetReputationPositionAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        var userReputation = await context
            .UserReputations.AsNoTracking()
            .Where(u => u.UserId == userId && u.ChatId == chatId)
            .Select(u => u.Reputation)
            .FirstOrDefaultAsync(token);

        var higherRankedUsersCount = await context
            .UserReputations.AsNoTracking()
            .Where(u => u.ChatId == chatId && u.Reputation > userReputation)
            .CountAsync(token);

        return higherRankedUsersCount + 1;
    }

    public async Task<IEnumerable<UserReputationDto>> GetTopUsersByReputationAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        var users = await context
            .UserReputations.AsNoTracking()
            .Where(u => u.ChatId == chatId)
            .OrderByDescending(u => u.Reputation)
            .Skip(skip)
            .Take(take)
            .ToListAsync(token);

        return users.Select(
            (user, index) =>
                new UserReputationDto(skip + index + 1, user.FirstName, user.Reputation)
        );
    }

    public async Task UpdateUserAsync(UserReputation user, CancellationToken token)
    {
        var existingUser = context.UserReputations.Local.FirstOrDefault(
            u => u.UserId == user.UserId && u.ChatId == user.ChatId
        );

        if (existingUser != null)
        {
            context.Entry(existingUser).CurrentValues.SetValues(user);
        }
        else
        {
            context.UserReputations.Update(user);
        }

        await context.SaveChangesAsync(token);
    }
}
