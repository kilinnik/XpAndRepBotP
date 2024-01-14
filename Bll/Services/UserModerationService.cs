using Bll.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Services;

public class UserModerationService(
    ITelegramBotClient botClient,
    IUserModerationRepository userModerationRepository
) : IUserModerationService
{
    public async Task<UserModeration> GetUserAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        return await userModerationRepository.GetUserAsync(userId, chatId, token);
    }

    public async Task BanUserAsync(long userId, long chatId, CancellationToken token)
    {
        await botClient.BanChatMemberAsync(chatId, userId, cancellationToken: token);
    }

    public async Task<string> UnbanUserAsync(long userId, long chatId, CancellationToken token)
    {
        await botClient.UnbanChatMemberAsync(chatId, userId, cancellationToken: token);
        return "Пользователь разбанен.";
    }

    public async Task MuteUserAsync(
        long userId,
        long chatId,
        TimeSpan duration,
        CancellationToken token
    )
    {
        var permissions = new ChatPermissions
        {
            CanSendMessages = false,
            CanSendAudios = false,
            CanSendDocuments = false,
            CanSendPhotos = false,
            CanSendPolls = false,
            CanSendVideoNotes = false,
            CanSendVideos = false,
            CanSendVoiceNotes = false,
            CanSendOtherMessages = false,
            CanAddWebPagePreviews = false,
            CanInviteUsers = false,
            CanPinMessages = false,
            CanChangeInfo = false,
            CanManageTopics = false
        };

        var muteUntil = DateTime.UtcNow.Add(duration);
        await botClient.RestrictChatMemberAsync(
            chatId,
            userId,
            permissions,
            untilDate: muteUntil,
            cancellationToken: token
        );
    }

    public async Task UnmuteUserAsync(long userId, long chatId, CancellationToken token)
    {
        var permissions = new ChatPermissions
        {
            CanSendMessages = true,
            CanSendAudios = true,
            CanSendDocuments = true,
            CanSendPhotos = true,
            CanSendPolls = true,
            CanSendVideoNotes = true,
            CanSendVideos = true,
            CanSendVoiceNotes = true,
            CanSendOtherMessages = true,
            CanAddWebPagePreviews = true,
            CanInviteUsers = true,
            CanPinMessages = true,
            CanManageTopics = true,
            CanChangeInfo = true
        };

        await botClient.RestrictChatMemberAsync(
            chatId,
            userId,
            permissions,
            cancellationToken: token
        );
    }

    public async Task<string> CheckAndRemoveWarnIfNeeded(
        UserModeration user,
        int warnDays,
        CancellationToken token
    )
    {
        if (!NeedRemoveWarn(user, warnDays))
        {
            return string.Empty;
        }

        user.WarnLastTime = DateTime.UtcNow;
        user.WarnCount = Math.Max(0, user.WarnCount - 1);

        await userModerationRepository.UpdateUserAsync(user, token);

        return $"С {user.FirstName} снято предупреждение. Всего предупреждений: {user.WarnCount}.";
    }

    private static bool NeedRemoveWarn(UserModeration user, int warnDays)
    {
        return user.WarnCount > 0
            && ((TimeSpan)(DateTime.Now - user.WarnLastTime)).TotalDays >= warnDays;
    }

    public async Task<string> WarnUserAsync(UserModeration user, CancellationToken token)
    {
        user.WarnCount += 1;
        user.WarnLastTime = DateTime.UtcNow;

        await userModerationRepository.UpdateUserAsync(user, token);

        return $"{user.FirstName} получил предупреждение. Всего предупреждений: {user.WarnCount}.";
    }

    public async Task<string> UnwarnUserAsync(UserModeration user, CancellationToken token)
    {
        user.WarnLastTime = DateTime.UtcNow;
        user.WarnCount = Math.Max(0, user.WarnCount - 1);

        await userModerationRepository.UpdateUserAsync(user, token);

        return $"С {user.FirstName} снято предупреждение. Всего предупреждений: {user.WarnCount}.";
    }
}
