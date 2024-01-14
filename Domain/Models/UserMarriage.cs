namespace Domain.Models;

public class UserMarriage(long userId, long chatId, string firstName, long partnerId)
{
    public long UserId { get; init; } = userId;
    public long ChatId { get; init; } = chatId;
    public string FirstName { get; set; } = firstName;
    public long PartnerId { get; init; } = partnerId;
    public DateTime MarriageDate { get; init; } = DateTime.UtcNow;
    public virtual UserMarriage Partner { get; set; }
}