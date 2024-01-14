using System.Text.RegularExpressions;
using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;

namespace Bll.Services;

public partial class UserLexiconService(IUserLexiconRepository userLexiconRepository)
    : IUserLexiconService
{
    public async Task<int> GetUserLexiconCountAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        return await userLexiconRepository.GetUserLexiconCountAsync(userId, chatId, token);
    }

    public async Task<IEnumerable<WordDto>> GetTopUserWordsAsync(
        long userId,
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        return await userLexiconRepository.GetTopUserWordsAsync(userId, chatId, skip, take, token);
    }

    public async Task<int> GetLexiconPositionAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        return await userLexiconRepository.GetLexiconPositionAsync(userId, chatId, token);
    }

    public async Task<IEnumerable<UserLexiconRankingDto>> GetTopUsersByLexiconAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        return await userLexiconRepository.GetTopUsersByLexiconAsync(chatId, skip, take, token);
    }

    public async Task<WordDto?> GetWordUsageAsync(
        long userId,
        long chatId,
        string word,
        CancellationToken token
    )
    {
        return await userLexiconRepository.GetWordUsageAsync(userId, chatId, word, token);
    }

    public async Task UpdateWordUsageAsync(
        long userId,
        long chatId,
        string messageText,
        CancellationToken token
    )
    {
        var words = messageText
            .Split([" ", "\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => MyRegex().Replace(word, ""))
            .Where(cleanedWord => !string.IsNullOrWhiteSpace(cleanedWord))
            .Select(
                cleanedWord => cleanedWord.Length > 100 ? cleanedWord[..100] : cleanedWord.ToLower()
            );

        foreach (var word in words)
        {
            await userLexiconRepository.UpdateWordUsageAsync(userId, chatId, word, token);
        }
    }

    public async Task<IEnumerable<WordDto>> GetTopWordsAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        return await userLexiconRepository.GetTopWordsAsync(chatId, skip, take, token);
    }

    public async Task<WordDto?> GetSpecificWordUsageAsync(
        long chatId,
        string word,
        CancellationToken token
    )
    {
        return await userLexiconRepository.GetSpecificWordUsageAsync(chatId, word, token);
    }

    [GeneratedRegex(@"[^\w\d\s]")]
    private static partial Regex MyRegex();
}
