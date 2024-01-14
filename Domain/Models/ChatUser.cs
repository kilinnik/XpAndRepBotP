namespace Domain.Models;

public class ChatUser(long userId, long chatId, string firstName, string username)
{
    public long UserId { get; init; } = userId;
    public long ChatId { get; init; } = chatId;
    public string FirstName { get; init; } = firstName;
    public string Username { get; set; } = username;
    public DateTime LastMessageDate { get; set; } = DateTime.UtcNow;
}
