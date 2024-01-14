using System.Text.RegularExpressions;
using Bll.Interfaces;
using Domain.DTO;
using Domain.Models;
using Infrastructure.Interfaces;
using Telegram.Bot.Types;

namespace Bll.Services;

public class UserReputationService(
    WordListService wordListService,
    IUserReputationRepository userReputationRepository
) : IUserReputationService
{
    public async Task<UserReputation> GetUserAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        return await userReputationRepository.GetUserReputationAsync(userId, chatId, token);
    }

    public async Task<string> HandleReputationUpAsync(Message message, CancellationToken token)
    {
        var messageText = message.Text ?? message.Caption ?? string.Empty;

        if (!IsReputationUp(messageText))
        {
            return string.Empty;
        }

        if (
            message is not { From: not null, ReplyToMessage.From: not null }
            || message.ReplyToMessage.From.Id == message.From.Id
            || message.ReplyToMessage.From.IsBot
        )
        {
            return string.Empty;
        }

        var user = await userReputationRepository.GetUserReputationAsync(
            message.ReplyToMessage.From.Id,
            message.Chat.Id,
            token
        );

        user.Reputation++;
        await userReputationRepository.UpdateUserAsync(user, token);

        return $"{message.From.FirstName} увеличил репутацию {user.FirstName} на 1({user.Reputation})";
    }

    public Task<int> GetReputationPositionAsync(long userId, long chatId, CancellationToken token)
    {
        return userReputationRepository.GetReputationPositionAsync(userId, chatId, token);
    }

    public async Task<IEnumerable<UserReputationDto>> GetTopUsersByReputationAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        return await userReputationRepository.GetTopUsersByReputationAsync(
            chatId,
            skip,
            take,
            token
        );
    }

    private bool IsReputationUp(string messageText)
    {
        var lowerCaseMessage = messageText.ToLower();

        var wordMatch = wordListService
            .ReputationWords.Where(word => word.Any(char.IsLetterOrDigit))
            .Select(repWord => @"\b" + Regex.Escape(repWord.ToLower()) + @"\b")
            .Any(pattern => Regex.IsMatch(lowerCaseMessage, pattern));

        if (wordMatch)
            return true;

        var symbolMatch = wordListService
            .ReputationWords.Where(word => !word.Any(char.IsLetterOrDigit))
            .Any(
                repWord =>
                    lowerCaseMessage.Contains(repWord, StringComparison.CurrentCultureIgnoreCase)
            );

        return symbolMatch;
    }
}
