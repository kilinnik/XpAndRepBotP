using Domain.DTO;
using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserLexiconRepository(IDbContextFactory<XpAndRepBotDbContext> contextFactory)
    : IUserLexiconRepository
{
    private readonly XpAndRepBotDbContext _context = contextFactory.CreateDbContext();

    private IQueryable<UserWord> GetUserWordsQuery(long userId, long chatId)
    {
        return _context
            .UserWords.AsNoTracking()
            .Where(ul => ul.UserId == userId && ul.ChatId == chatId);
    }

    public async Task UpdateWordUsageAsync(
        long userId,
        long chatId,
        string word,
        CancellationToken token
    )
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(token);

            var userWord = await context.UserWords.FirstOrDefaultAsync(
                ul => ul.UserId == userId && ul.ChatId == chatId && ul.Word == word,
                token
            );

            if (userWord == null)
            {
                userWord = new UserWord
                {
                    UserId = userId,
                    ChatId = chatId,
                    Word = word,
                    WordCount = 1
                };
                context.UserWords.Add(userWord);
            }
            else
            {
                userWord.WordCount++;
            }

            await context.SaveChangesAsync(token);
        }
        catch (Exception e)
        {
            Console.WriteLine($"User: {userId}, Chat: {chatId}, Word: {word}\nError: {e}");
        }
    }

    public async Task<int> GetUserLexiconCountAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        return await GetUserWordsQuery(userId, chatId).CountAsync(token);
    }

    public async Task<IEnumerable<WordDto>> GetTopUserWordsAsync(
        long userId,
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        var wordUsageQuery = GetUserWordsQuery(userId, chatId)
            .OrderByDescending(ul => ul.WordCount)
            .Skip(skip)
            .Take(take);

        var wordUsageList = await wordUsageQuery.ToListAsync(token);

        return wordUsageList.Select(
            (wordUsage, index) => new WordDto(wordUsage.Word, wordUsage.WordCount, skip + index + 1)
        );
    }

    public async Task<IEnumerable<UserLexiconRankingDto>> GetTopUsersByLexiconAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        var userWordCounts = await _context
            .UserWords.AsNoTracking()
            .Where(uw => uw.ChatId == chatId)
            .GroupBy(uw => uw.UserId)
            .Select(group => new { UserId = group.Key, WordCount = group.Count() })
            .OrderByDescending(g => g.WordCount)
            .Skip(skip)
            .Take(take)
            .ToListAsync(token);

        var userLexiconRankingDtos = new List<UserLexiconRankingDto>();

        var rowNumber = skip + 1;
        foreach (var userWordCount in userWordCounts)
        {
            var userName = await _context
                .Users.Where(u => u.UserId == userWordCount.UserId && u.ChatId == chatId)
                .Select(u => u.FirstName)
                .FirstOrDefaultAsync(token);

            userLexiconRankingDtos.Add(
                new UserLexiconRankingDto(rowNumber++, userName, userWordCount.WordCount)
            );
        }

        return userLexiconRankingDtos;
    }

    public async Task<int> GetLexiconPositionAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        var userUniqueWordCount = await GetUserWordsQuery(userId, chatId)
            .Select(uw => uw.Word)
            .Distinct()
            .CountAsync(token);

        var higherRankedUsersCount = await _context
            .UserWords.AsNoTracking()
            .Where(uw => uw.ChatId == chatId)
            .GroupBy(uw => uw.UserId)
            .Select(
                g =>
                    new
                    {
                        UserId = g.Key,
                        UniqueWordCount = g.Select(x => x.Word).Distinct().Count()
                    }
            )
            .CountAsync(u => u.UniqueWordCount > userUniqueWordCount, token);

        return higherRankedUsersCount + 1;
    }

    public async Task<WordDto?> GetWordUsageAsync(
        long userId,
        long chatId,
        string word,
        CancellationToken token
    )
    {
        var wordUsage = await GetUserWordsQuery(userId, chatId)
            .FirstOrDefaultAsync(ul => ul.Word == word, token);

        if (wordUsage == null)
        {
            return null;
        }

        var rank =
            await _context
                .UserWords.AsNoTracking()
                .Where(ul => ul.ChatId == chatId && ul.WordCount > wordUsage.WordCount)
                .CountAsync(token) + 1;

        return new WordDto(wordUsage.Word, wordUsage.WordCount, rank);
    }

    public async Task<IEnumerable<WordDto>> GetTopWordsAsync(
        long chatId,
        int skip,
        int take,
        CancellationToken token
    )
    {
        var words = await _context
            .UserWords.AsNoTracking()
            .Where(ul => ul.ChatId == chatId)
            .GroupBy(ul => ul.Word)
            .Select(group => new { Word = group.Key, WordCount = group.Sum(g => g.WordCount) })
            .OrderByDescending(twd => twd.WordCount)
            .Skip(skip)
            .Take(take)
            .ToListAsync(token);

        return words.Select(
            (word, index) => new WordDto(word.Word, word.WordCount, skip + index + 1)
        );
    }

    public async Task<WordDto?> GetSpecificWordUsageAsync(
        long chatId,
        string word,
        CancellationToken token
    )
    {
        var wordCount = await _context
            .UserWords.AsNoTracking()
            .Where(ul => ul.ChatId == chatId && ul.Word == word)
            .SumAsync(ul => ul.WordCount, token);

        var totalWords = await _context
            .UserWords.AsNoTracking()
            .Where(ul => ul.ChatId == chatId)
            .GroupBy(ul => ul.Word)
            .Select(group => new { group.Key, Count = group.Sum(g => g.WordCount) })
            .OrderByDescending(t => t.Count)
            .ToListAsync(token);

        var rank = totalWords.FindIndex(w => w.Key == word) + 1;

        return rank == 0 ? null : new WordDto(word, wordCount, rank);
    }
}
