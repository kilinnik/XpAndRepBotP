using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Uthef.FusionBrain;
using Uthef.FusionBrain.Extensions;
using Uthef.FusionBrain.Types;

namespace Infrastructure.Services;

public class ImageGenerationService(FusionBrainApi api, ILogger<ImageGenerationService> logger)
    : IImageGenerationService
{
    public async Task<InputFileStream?> GenerateImageAsync(string prompt)
    {
        var models = await api.GetModelsAsync();
        var kandinskyModel = models.FirstOrDefault(m => m.Name == "Kandinsky");

        if (kandinskyModel == null)
        {
            logger.LogError("Kandinsky model not found");
            return null;
        }

        var promptRequest = new Prompt(kandinskyModel, prompt);
        var firstStatus = await api.GenerateAsync(promptRequest);

        var finalStatus = await api.PollAsync(firstStatus.Uuid);

        if (finalStatus is { Images: not null, Failed: false } && finalStatus.Images.Any())
        {
            var imageBytes = Convert.FromBase64String(finalStatus.Images.First());
            var stream = new MemoryStream(imageBytes);
            return new InputFileStream(stream, "image.png");
        }

        logger.LogError(
            "Image generation failed: {FinalStatusErrorDescription}",
            finalStatus.ErrorDescription
        );
        return null;
    }
}
