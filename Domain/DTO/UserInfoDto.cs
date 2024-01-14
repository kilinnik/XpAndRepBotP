using System.Text;
using Domain.Common;

namespace Domain.DTO;

public record UserInfoDto(
    string Name,
    int Level,
    int CurrentExperience,
    int NextLevelExperience,
    int Reputation,
    int LexiconCount,
    int LevelPosition,
    int ReputationPosition,
    int LexiconPosition,
    string Roles,
    DateTime LastMessageTime,
    int Warns,
    DateTime? LastWarnTime,
    IEnumerable<WordDto> TopWords
)
{
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"👨‍❤️‍👨 Имя: {Name}");
        builder.AppendLine($"⭐️ Уровень: {Level} ({CurrentExperience} / {NextLevelExperience} XP)");
        builder.AppendLine($"😇 Репутация: {Reputation}");
        builder.AppendLine($"🔤 Лексикон: {LexiconCount} слов");
        builder.AppendLine($"🏆 Место в топе по уровню: {LevelPosition}");
        builder.AppendLine($"🥇 Место в топе по репутации: {ReputationPosition}");
        builder.AppendLine($"🎖 Место в топе по лексикону: {LexiconPosition}");
        builder.AppendLine($"🎭 Роли: {Roles}");
        builder.AppendLine($"🤬 Кол-во варнов: {Warns}/3");
        builder.AppendLine(
            $"🗓 Дата последнего варна/снятия варна: {(LastWarnTime.HasValue ? LastWarnTime.Value.ToString("yy/MM/dd HH:mm:ss") : string.Empty)}"
        );
        builder.AppendLine($"🕰 Дата последнего сообщения: {LastMessageTime:yy/MM/dd HH:mm:ss}");
        builder.AppendLine("📖 Личный топ слов:");

        var rank = 1;
        foreach (var word in TopWords)
        {
            var rankEmoji = Utils.NumberToEmoji(rank);
            builder.AppendLine($"{rankEmoji} {word.Word} || {word.WordCount}");
            rank++;
        }

        return builder.ToString();
    }
}