namespace Domain.Models;

public class UserModeration(long userId, long chatId, string firstName)
{
    public long UserId { get; set; } = userId;
    public long ChatId { get; set; } = chatId;
    public string FirstName { get; set; } = firstName;
    public int WarnCount { get; set; }
    public DateTime? WarnLastTime { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BanDate { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public int CountRepeatLastMessage { get; set; } = 1;
}