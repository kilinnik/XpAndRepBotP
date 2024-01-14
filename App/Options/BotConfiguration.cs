namespace App.Options;

public class BotConfiguration
{
    public const string Section = nameof(BotConfiguration);

    public string Token { get; init; } = string.Empty;
}
