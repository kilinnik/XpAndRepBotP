namespace Domain.Models;

public class UserRole(long userId, long chatId, string firstName, string roles)
{
    public long UserId { get; init; } = userId;
    public long ChatId { get; init; } = chatId;
    public string FirstName { get; init; } = firstName;
    public string Roles { get; set; } = roles;
}