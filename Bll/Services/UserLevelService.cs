using System.Text;
using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Infrastructure.Interfaces;

namespace Bll.Services;

public class UserLevelService(IUserLevelRepository userLevelRepository) : IUserLevelService
{
    public Task<UserLevel> GetUserLevelAsync(long userId, long chatId, CancellationToken token)
    {
        return userLevelRepository.GetUserLevelAsync(userId, chatId, token);
    }

    public Task<int> GetLevelPositionAsync(long userId, long chatId, CancellationToken token)
    {
        return userLevelRepository.GetLevelPositionAsync(userId, chatId, token);
    }

    public async Task<IEnumerable<UserLevelDto>> GetTopUsersByLevelAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        return await userLevelRepository.GetTopUsersByLevelAsync(chatId, skip, take, token);
    }

    public async Task<string> HandleLevelUpAsync(UserLevel userLevel, CancellationToken token)
    {
        var levelUpMessages = new StringBuilder();

        while (userLevel.CurrentExperience >= Utils.XpForLvlUp[userLevel.Level])
        {
            userLevel.CurrentExperience -= Utils.XpForLvlUp[userLevel.Level++];
            levelUpMessages.AppendLine($"{userLevel.FirstName} получает {userLevel.Level} lvl");
        }

        await userLevelRepository.UpdateUserLevelAsync(userLevel, token);
        return levelUpMessages.ToString().TrimEnd();
    }
}