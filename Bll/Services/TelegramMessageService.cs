using Bll.Interfaces;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace Bll.Services;

public class TelegramMessageService(
    ITelegramBotClient botClient,
    ILogger<TelegramMessageService> logger
) : ITelegramMessageService
{
    public async Task SendMessageAsync(
        CommandResult commandResult,
        CancellationToken token = default
    )
    {
        if (commandResult.Texts != null)
        {
            var tasks = commandResult.Texts.Select(
                (text, index) => SendTextMessageAsync(text, index, commandResult, token)
            );
            await Task.WhenAll(tasks);
        }
        else if (commandResult.Photo != null)
        {
            try
            {
                await botClient.SendPhotoAsync(
                    chatId: commandResult.ChatId,
                    photo: commandResult.Photo,
                    replyToMessageId: commandResult.ReplyToMessageId,
                    cancellationToken: token
                );
            }
            catch (ApiRequestException ex) when (ex.Message.Contains("message to reply not found"))
            {
                logger.LogWarning("Attempted to reply to a message that does not exist");
                await botClient.SendPhotoAsync(
                    chatId: commandResult.ChatId,
                    photo: commandResult.Photo,
                    cancellationToken: token
                );
            }
        }
    }

    private async Task SendTextMessageAsync(
        string text,
        int index,
        CommandResult commandResult,
        CancellationToken token
    )
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var keyboard =
            commandResult.InlineKeyboards?.Count > index
                ? commandResult.InlineKeyboards[index]
                : null;

        try
        {
            await botClient.SendTextMessageAsync(
                chatId: commandResult.ChatId,
                text: text,
                replyToMessageId: commandResult.ReplyToMessageId,
                replyMarkup: keyboard,
                parseMode: commandResult.ParseMode,
                cancellationToken: token
            );
        }
        catch (ApiRequestException ex) when (ex.Message.Contains("message to reply not found"))
        {
            logger.LogWarning("Attempted to reply to a message that does not exist");
            await botClient.SendTextMessageAsync(
                chatId: commandResult.ChatId,
                text: text,
                replyMarkup: keyboard,
                parseMode: commandResult.ParseMode,
                cancellationToken: token
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending message: {Text}", text);
        }
    }
}
