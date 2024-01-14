using System.Text;
using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Infrastructure.Interfaces;

namespace Bll.Services;

public class UserProfileService(
    IUserRepository userRepository,
    IUserLevelService userLevelService,
    IUserLexiconService userLexiconService,
    IUserReputationService userReputationService,
    IUserModerationRepository userModerationRepository, 
    IUserMarriageService userMarriageService,
    IUserRoleService userRoleService
) : IUserProfileService
{
    public async Task<UserInfoDto> GetUserInfoAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        var user = await userRepository.GetUserAsync(userId, chatId, token);
        var levelPosition = await userLevelService.GetLevelPositionAsync(userId, chatId, token);
        var lexiconCount = await userLexiconService.GetUserLexiconCountAsync(userId, chatId, token);
        var lexiconPosition = await userLexiconService.GetLexiconPositionAsync(
            userId,
            chatId,
            token
        );
        var reputationPosition = await userReputationService.GetReputationPositionAsync(
            userId,
            chatId,
            token
        );
        var topWords = await userLexiconService.GetTopUserWordsAsync(userId, chatId, 0, 10, token);

        var userRole = await userRoleService.GetUserRoleAsync(userId, chatId, token);
        var userReputation = await userReputationService.GetUserAsync(userId, chatId, token);
        var userLevel = await userLevelService.GetUserLevelAsync(userId, chatId, token);
        var userModeration = await userModerationRepository.GetUserAsync(userId, chatId, token);
        

        var userInfoDto = new UserInfoDto(
            user.FirstName,
            userLevel.Level,
            userLevel.CurrentExperience,
            Utils.XpForLvlUp.Length > userLevel.Level
                ? Utils.XpForLvlUp[userLevel.Level]
                : int.MaxValue,
            userReputation.Reputation,
            lexiconCount,
            levelPosition,
            reputationPosition,
            lexiconPosition,
            userRole?.Roles ?? string.Empty,
            user.LastMessageDate,
            userModeration.WarnCount,
            userModeration.WarnLastTime,
            topWords
        );

        return userInfoDto;
    }

    public async Task<string> GetUserLexiconPageAsync(
        long userId,
        long chatId,
        int offset,
        CancellationToken token
    )
    {
        var topWords = await userLexiconService.GetTopUserWordsAsync(
            userId,
            chatId,
            offset,
            10,
            token
        );

        var builder = new StringBuilder();
        var rank = offset + 1;
        foreach (var word in topWords)
        {
            builder.AppendLine($"{Utils.NumberToEmoji(rank++)} {word.Word} || {word.WordCount}");
        }

        return builder.ToString();
    }
}
