namespace Domain.Models;

public class UserWord
{
    public long UserId { get; init; }
    public string Word { get; init; } = string.Empty;
    public int WordCount { get; set; }
    public long ChatId { get; init; }
}
