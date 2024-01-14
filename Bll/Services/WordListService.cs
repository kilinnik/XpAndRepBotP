using System.Reflection;

namespace Bll.Services;

public class WordListService
{
    private static readonly char[] Separator = [','];
    public HashSet<string> BannedWords { get; }
    public HashSet<string> ForbiddenPhrases { get; }

    public IEnumerable<string> ReputationWords { get; } =
        [
            "+",
            "спс",
            "спасибо",
            "спасиб",
            "спасиба",
            "пасиб",
            "пасибо",
            "пасиба",
            "сяб",
            "сяба",
            "сяби",
            "молодчик",
            "хорош",
            "благодарю",
            "класс",
            "молодец",
            "жиза",
            "гц",
            "грац",
            "дяк",
            "дякую",
            "база",
            "соглы",
            "thx",
            "thanx",
            "thanks",
            "thank you",
            "thank u",
            "danke schön",
            "danke",
            "данке",
            "viele danke",
            "рахмет",
            "👍",
            "👍🏻",
            "👍🏼",
            "👍🏽",
            "👍🏾",
            "👍🏿",
            "aitäh",
            "merci",
            "joué",
            "bien joué",
            "вдячний"
        ];

    private WordListService(HashSet<string> bannedWords, HashSet<string> forbiddenPhrases)
    {
        BannedWords = bannedWords;
        ForbiddenPhrases = forbiddenPhrases;
    }

    public static async Task<WordListService> CreateAsync()
    {
        var bannedWords = await ReadResourceLinesToLower("Bll.Resources.bw.txt");
        var forbiddenPhrases = LoadForbiddenPhrases();
        return new WordListService(bannedWords, forbiddenPhrases);
    }

    private static HashSet<string> LoadForbiddenPhrases()
    {
        var forbiddenPhrasesResource = Resources.ForbiddenWords;
        return
        [
            ..forbiddenPhrasesResource.Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(phrase => phrase.Trim().ToLower())
        ];
    }

    private static async Task<HashSet<string>> ReadResourceLinesToLower(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new Exception($"Resource {resourceName} not found.");

        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            lines.Add(await reader.ReadLineAsync() ?? string.Empty);
        }

        return [..lines.Select(line => line.ToLower())];
    }
}
