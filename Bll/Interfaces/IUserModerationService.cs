using Domain.Models;

namespace Bll.Interfaces;

public interface IUserModerationService
{
    Task<UserModeration> GetUserAsync(long userId, long chatId, CancellationToken token);

    Task BanUserAsync(long userId, long chatId, CancellationToken token);

    Task<string> UnbanUserAsync(long userId, long chatId, CancellationToken token);

    Task MuteUserAsync(long userId, long chatId, TimeSpan duration, CancellationToken token);

    Task UnmuteUserAsync(long userId, long chatId, CancellationToken token);

    Task<string> CheckAndRemoveWarnIfNeeded(
        UserModeration user,
        int warnDays,
        CancellationToken token
    );

    Task<string> WarnUserAsync(UserModeration user, CancellationToken token);

    Task<string> UnwarnUserAsync(UserModeration user, CancellationToken token);
}
