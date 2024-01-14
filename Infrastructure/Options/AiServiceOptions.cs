using Junaid.GoogleGemini.Net.Models.GoogleApi;

namespace Infrastructure.Options;

public class AiServiceOptions
{
    public const string Section = nameof(AiServiceOptions);

    public string ApiKey { get; init; } = string.Empty;
    public double Temperature { get; init; }
    public double TopP { get; init; }
    public int TopK { get; init; }
    public int MaxOutputTokens { get; init; }
    public List<SafetySetting> SafetySettings { get; init; } = [];
    public string BaseUrl { get; init; } = string.Empty;
}
