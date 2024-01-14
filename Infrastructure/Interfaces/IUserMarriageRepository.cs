using Domain.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Interfaces;

public interface IUserMarriageRepository
{
    Task CreateUserMarriageAsync(UserMarriage userMarriage, CancellationToken token);
    
    Task<UserMarriage> GetUserMarriageAsync(long userId, long chatId, CancellationToken token);
    
    Task<List<UserMarriage>> GetMarriedUsersWithPartnersAsync(long chatId, CancellationToken token);
    
    Task UpdateUserMarriageAsync(UserMarriage userMarriage, CancellationToken token);
    
    Task RemoveUserMarriageAsync(UserMarriage userMarriage, CancellationToken token);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token);
}
