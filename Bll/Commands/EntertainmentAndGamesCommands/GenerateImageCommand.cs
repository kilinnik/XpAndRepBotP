using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bll.Commands.EntertainmentAndGamesCommands;

public class GenerateImageCommand(
    IImageGenerationService imageGenerationService,
    ILogger<GenerateImageCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var photo = await imageGenerationService.GenerateImageAsync(message.Text);

            return new CommandResult(message.Chat.Id, null, message.MessageId, Photo: photo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GenerateImageCommand");
            return new CommandResult(message.Chat.Id, new List<string> { "Не удалось сгенерировать изображение" },
                message.MessageId);
        }
    }
}