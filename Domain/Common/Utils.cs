using System.Text;
using Domain.DTO;

namespace Domain.Common;

public static class Utils
{
    private const string Keycap_0 = "0️⃣";
    private const string Keycap_1 = "1️⃣";
    private const string Keycap_2 = "2️⃣";
    private const string Keycap_3 = "3️⃣";
    private const string Keycap_4 = "4️⃣";
    private const string Keycap_5 = "5️⃣";
    private const string Keycap_6 = "6️⃣";
    private const string Keycap_7 = "7️⃣";
    private const string Keycap_8 = "8️⃣";
    private const string Keycap_9 = "9️⃣";
    private const string Keycap_10 = "🔟";

    private static readonly string[] KeycodeKeymaps =
    [
        Keycap_0,
        Keycap_1,
        Keycap_2,
        Keycap_3,
        Keycap_4,
        Keycap_5,
        Keycap_6,
        Keycap_7,
        Keycap_8,
        Keycap_9,
        Keycap_10
    ];

    public static readonly int[] XpForLvlUp =
    [
        100,
        235,
        505,
        810,
        1250,
        1725,
        2335,
        2980,
        3760,
        4575,
        5525,
        6510,
        7630,
        8785,
        10075,
        11400,
        12860,
        14355,
        15985,
        17650,
        19450,
        21285,
        23255,
        25260,
        27400,
        29575,
        31885,
        34230,
        36710,
        39225,
        41875,
        44560,
        47380,
        50235,
        53225,
        56250,
        59410,
        62605,
        65935,
        69300,
        72800,
        76335,
        80005,
        83710,
        87550,
        91425,
        95435,
        99480,
        103660,
        107875,
        111975,
        116050,
        120100,
        124125,
        128125,
        132100,
        136050,
        139975,
        143875,
        147750,
        151600,
        155425,
        159225,
        163000,
        166750,
        170475,
        174175,
        177850,
        181500,
        185125,
        188725
    ];

    public static string NumberToEmoji(int number)
    {
        if (number == 10)
        {
            return Keycap_10;
        }

        var digits = number.ToString().Select(digit => KeycodeKeymaps[int.Parse(digit.ToString())]);
        return string.Join("", digits);
    }

    public static bool IsUserModerator(string? userRoles)
    {
        return userRoles != null && userRoles.Contains("модер");
    }

    public static string FormatTopLevelUsers(IEnumerable<UserLevelDto> users, int offset)
    {
        var sb = new StringBuilder("🏆 Топ пользователей по уровню:\n");
        var rank = 1 + offset;
        foreach (var user in users)
        {
            sb.AppendLine(
                $"{NumberToEmoji(rank++)} {user.Name} | lvl {user.Level} | {user.CurrentExperience}/{user.NextLevelExperience}"
            );
        }

        return sb.ToString();
    }

    public static string FormatTopWords(IEnumerable<WordDto> words, int offset)
    {
        var sb = new StringBuilder("📚 <b>Топ слов:</b>\n");
        foreach (var word in words)
        {
            sb.Append($"{NumberToEmoji(word.RowNumber)} <b>{word.Word}</b> - {word.WordCount}\n");
        }

        return sb.ToString();
    }

    public static string FormatRoles(IEnumerable<RoleDto> roles, int offset)
    {
        var sb = new StringBuilder("🎭 <b>Топ ролей:</b>\n");
        var rank = 1 + offset;
        foreach (var role in roles)
        {
            sb.AppendLine(
                $"{NumberToEmoji(rank++)} <code>{role.RoleName}</code>: {string.Join(", ", role.UserNames)}"
            );
        }

        return sb.ToString();
    }

    public static string FormatTopUsersByReputation(
        IEnumerable<UserReputationDto> users,
        int offset
    )
    {
        var sb = new StringBuilder("🌟 <b>Топ по репутации:</b>\n");
        var rank = 1 + offset;
        foreach (var user in users)
        {
            sb.AppendLine(
                $"{NumberToEmoji(rank++)} <b>{user.UserName}</b> - rep: {user.Reputation}"
            );
        }

        return sb.ToString();
    }

    public static string FormatTopUsersByLexicon(
        IEnumerable<UserLexiconRankingDto> users,
        int offset
    )
    {
        var sb = new StringBuilder("🎖 <b>Топ по лексикону:</b>\n");
        var rank = 1 + offset;
        foreach (var user in users)
        {
            sb.AppendLine(
                $"{NumberToEmoji(rank++)} <b>{user.UserName}</b> - слов: {user.WordCount}"
            );
        }

        return sb.ToString();
    }
}
