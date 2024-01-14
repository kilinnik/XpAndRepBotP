namespace Domain.Models;

public class UserReputation(long userId, long chatId, string firstName)
{
    public long UserId { get; set; } = userId;
    public long ChatId { get; set; } = chatId;
    public string FirstName { get; set; } = firstName;
    public int Reputation { get; set; }
}