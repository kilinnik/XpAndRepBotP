using Domain.DTO;

namespace Bll.Interfaces;

public interface IUserLexiconService
{
    Task<int> GetUserLexiconCountAsync(long userId, long chatId, CancellationToken token);

    Task<IEnumerable<WordDto>> GetTopUserWordsAsync(long userId, long chatId, int skip, int take,
        CancellationToken token);

    Task<int> GetLexiconPositionAsync(long userId, long chatId, CancellationToken token);

    Task<IEnumerable<UserLexiconRankingDto>> GetTopUsersByLexiconAsync(long chatId, int skip, int take,
        CancellationToken token);
    
    Task<WordDto?> GetWordUsageAsync(long userId, long chatId, string word, CancellationToken token);
    
    Task<IEnumerable<WordDto>> GetTopWordsAsync(long chatId, int skip, int take, CancellationToken token);
    
    Task<WordDto?> GetSpecificWordUsageAsync(long chatId, string word, CancellationToken token);
    
    Task UpdateWordUsageAsync(long userId, long chatId, string messageText, CancellationToken token);
}