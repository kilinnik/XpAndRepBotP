namespace Domain.Models;

public class UserLevel(long userId, long chatId, string firstName)
{
    public long UserId { get; init; } = userId;
    public long ChatId { get; init; } = chatId;
    public string FirstName { get; set; } = firstName;
    public int Level { get; set; }
    public int CurrentExperience { get; set; }
}