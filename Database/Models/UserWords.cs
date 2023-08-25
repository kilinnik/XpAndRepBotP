namespace XpAndRepBot.Database.Models;

public class UserWords
{
    public long UserId { get; set; }

    public string Word { get; set; }

    public int Count { get; set; }
    public long ChatId { get; set; }
    public Chat Chat { get; set; }
}