namespace Infrastructure.Options;

public class ConnectionStrings
{
    public const string Section = nameof(ConnectionStrings);

    public string XpAndRepBot { get; init; } = string.Empty;
}
