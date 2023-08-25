using System.ComponentModel.DataAnnotations;

namespace XpAndRepBot.Database.Models;


public class Chat
{
    [Key] public long ChatId { get; set; }
    public string Rules { get; set; }
    public string HelpCommands { get; set; }
    public string Greeting { get; set; }
    public string HelpAdminCommands { get; set; }
    public int WarnDays { get; set; }
}