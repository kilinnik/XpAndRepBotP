namespace Domain.Models;

public class UserNfc(long userId, long chatId, string firstName, DateTime startNfcDate)
{
    public long UserId { get; set; } = userId;
    public long ChatId { get; set; } = chatId;
    public string FirstName { get; set; } = firstName;
    public bool IsNfcActive { get; set; } = true;
    public DateTime StartNfcDate { get; set; } = startNfcDate;
    public long NfcBestTime { get; set; }
}