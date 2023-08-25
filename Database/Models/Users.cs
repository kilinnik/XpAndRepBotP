using System;

namespace XpAndRepBot.Database.Models;

public class Users
{
    private static readonly DateTime DefaultDate = DateTime.ParseExact("1900-01-01 00:00:00.000",
        "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

    public long UserId { get; set; }
    public string Name { get; set; }
    public int Lvl { get; set; }
    public int CurXp { get; set; }
    public int Rep { get; set; }
    public int Warns { get; set; }
    public DateTime LastTime { get; set; }
    public string Roles { get; set; }
    public bool? Nfc { get; set; }
    public DateTime StartNfc { get; set; }
    public long BestTime { get; set; }
    public DateTime TimeLastMes { get; set; }
    public string LastMessage { get; set; }
    public int CountRepeatMessage { get; set; }
    public long Mariage { get; set; }
    public DateTime DateMariage { get; set; }
    public string Username { get; set; }
    public DateTime DateMute { get; set; }
    public bool CheckEnter { get; set; }
    public string Complaints { get; set; }
    public string Complainers { get; set; }
    public long ChatId { get; set; }
    public Chat Chat { get; set; }

    public Users(long userId, string name, int lvl, int curXp, int rep, long chatId)
    {
        UserId = userId;
        Name = name;
        Lvl = lvl;
        CurXp = curXp;
        Rep = rep;
        Warns = 0;
        LastTime = DefaultDate;
        Nfc = false;
        StartNfc = DefaultDate;
        BestTime = 0;
        TimeLastMes = DefaultDate;
        CountRepeatMessage = 1;
        LastMessage = "";
        Mariage = 0;
        DateMariage = DefaultDate;
        Username = "";
        DateMute = DefaultDate;
        CheckEnter = true;
        Complaints = "";
        Complainers = "";
        ChatId = chatId;
    }
}