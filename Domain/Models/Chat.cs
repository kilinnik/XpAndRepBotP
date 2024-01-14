using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class Chat
{
    [Key]
    public long ChatId { get; init; }
    public string Rules { get; set; } = string.Empty;
    public string Greeting { get; set; } = string.Empty;
    public int WarnDays { get; set; }
    public bool? IsLinkMessageEnabled { get; set; } = false;
    public string? InviteLinkMessageText { get; set; } = "Приглашаем вас в чат!";
    public string? LinkButtonText { get; set; } = "Присоединиться";
    public string? LinkUrl { get; set; } = "https://t.me/yourchatlink";
    public bool? IsRestrictionEnabled { get; set; }
    public bool? IsContentModerationEnabled { get; set; }
    public bool IsRepeatedMessageCheckEnabled { get; set; }
    public string? LinkMessageText { get; set; }
    public string? LinkButtonNames { get; set; }
    public string? LinkUrls { get; set; }
}
