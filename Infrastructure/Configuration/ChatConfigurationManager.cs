using Infrastructure.Options;
using Junaid.GoogleGemini.Net.Models.GoogleApi;
using Microsoft.Extensions.Options;

namespace Infrastructure.Configuration;

public class ChatConfigurationManager(IOptions<AiServiceOptions> aiOptions)
{
    private readonly AiServiceOptions _aiOptions = aiOptions.Value;

    public GenerateContentConfiguration CreateChatConfiguration()
    {
        return new GenerateContentConfiguration
        {
            safetySettings = _aiOptions.SafetySettings.ToArray(),
            generationConfig = new GenerationConfig
            {
                temperature = _aiOptions.Temperature,
                topP = _aiOptions.TopP,
                topK = _aiOptions.TopK,
                maxOutputTokens = _aiOptions.MaxOutputTokens
            }
        };
    }
}
