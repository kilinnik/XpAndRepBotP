using Telegram.Bot.Types;

namespace Infrastructure.Interfaces;

public interface IImageGenerationService
{
    Task<InputFileStream?> GenerateImageAsync(string prompt);
}
