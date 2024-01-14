using Domain.Models;

namespace Bll.Interfaces;

public interface IUserMarriageService
{
    Task<UserMarriage> GetUserMarriageAsync(long userId, long chatId, CancellationToken token);

    Task<string> GetMarriagesAsync(long chatId, CancellationToken cancellationToken);

    Task<string> GetMarriageStatusAsync(long userId, long chatId, CancellationToken token);

    Task<string> UpdateMarriageStatus(
        long chatId,
        long proposerId,
        long proposeeId,
        bool isAccepted,
        CancellationToken token
    );

    Task<string> ProcessDivorceAsync(long userId, long chatId, CancellationToken cancellationToken);

    Task<string?> CheckMarriageStatus(
        long proposerId,
        long proposeeId,
        long chatId,
        CancellationToken token
    );
}
