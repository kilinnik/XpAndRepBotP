using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories;

public class UserMarriageRepository(XpAndRepBotDbContext context) : IUserMarriageRepository
{
    public async Task CreateUserMarriageAsync(UserMarriage userMarriage, CancellationToken token)
    {
        context.UserMarriages.Add(userMarriage);
        await context.SaveChangesAsync(token);
    }

    public async Task<UserMarriage> GetUserMarriageAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        return await context
            .UserMarriages.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.ChatId == chatId, token);
    }

    public async Task<List<UserMarriage>> GetMarriedUsersWithPartnersAsync(
        long chatId,
        CancellationToken token
    )
    {
        var marriedUsers = await context
            .UserMarriages.AsNoTracking()
            .Where(u => u.ChatId == chatId && u.PartnerId != 0)
            .Include(u => u.Partner)
            .ToListAsync(token);

        return marriedUsers;
    }

    public async Task UpdateUserMarriageAsync(UserMarriage userMarriage, CancellationToken token)
    {
        var existingEntity = context.UserMarriages.Local.FirstOrDefault(
            u => u.UserId == userMarriage.UserId && u.ChatId == userMarriage.ChatId
        );

        if (existingEntity != null)
        {
            context.Entry(existingEntity).CurrentValues.SetValues(userMarriage);
        }
        else
        {
            context.UserMarriages.Update(userMarriage);
        }

        await context.SaveChangesAsync(token);
    }

    public async Task RemoveUserMarriageAsync(UserMarriage userMarriage, CancellationToken token)
    {
        context.UserMarriages.Remove(userMarriage);
        await context.SaveChangesAsync(token);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token)
    {
        return await context.Database.BeginTransactionAsync(token);
    }
}