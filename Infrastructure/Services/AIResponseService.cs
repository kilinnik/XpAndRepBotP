using System.Text.RegularExpressions;
using Infrastructure.Configuration;
using Infrastructure.Interfaces;
using Junaid.GoogleGemini.Net.Models;
using Junaid.GoogleGemini.Net.Models.Requests;
using Junaid.GoogleGemini.Net.Services;
using Markdig;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Services;

public partial class AiResponseService(
    ChatService chatService,
    VisionService visionService,
    IChatThreadService threadService,
    ILogger<AiResponseService> logger,
    ChatConfigurationManager chatConfigurationManager
) : IAiResponseService
{
    private readonly List<long> _allowedUserIds = [859181523, 1813723228, 1882185833];

    public async Task HandleMessageAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken token
    )
    {
        if (!ShouldProcessMessage(message))
            return;

        var responseText = await ProcessMessageAsync(botClient, message, token);
        if (string.IsNullOrWhiteSpace(responseText))
            return;

        await SendMessageAsync(botClient, message, responseText, token);
    }

    private bool ShouldProcessMessage(Message message)
    {
        var isCommand = message.Entities?.Any(e => e.Type == MessageEntityType.BotCommand) ?? false;
        var containsBotMention = message.Text?.Contains("@XpAndRepBot") ?? false;
        var isAllowedUser = _allowedUserIds.Contains(message.Chat.Id);

        return (containsBotMention || IsReplyToBot(message) || isAllowedUser) && !isCommand;
    }

    private async Task<string> ProcessMessageAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken token
    )
    {
        var threadKey = threadService.GetThreadKey(message);
        var messageContent = ExtractContentFromMessage(message);
        
        if (string.IsNullOrWhiteSpace(messageContent))
        {
            return string.Empty;
        }

        threadService.AddMessageAndIdToThread(threadKey, messageContent, message.MessageId);

        return IsMediaMessage(message)
            ? await ProcessMediaMessageAsync(botClient, message, messageContent, token)
            : await GenerateChatResponse(threadKey);
    }

    private async Task SendMessageAsync(
        ITelegramBotClient botClient,
        Message message,
        string responseText,
        CancellationToken token
    )
    {
        try
        {
            responseText = ConvertToHtml(responseText);
            var sentMessage = await botClient.SendTextMessageAsync(
                message.Chat.Id,
                responseText,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html,
                cancellationToken: token
            );

            var threadKey = threadService.GetThreadKey(message);
            threadService.AddMessageAndIdToThread(
                threadKey,
                responseText,
                sentMessage.MessageId,
                true
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send message, ResponseText = {ResponseText}",
                responseText
            );
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Произошла ошибка при отправке сообщения.",
                replyToMessageId: message.MessageId,
                cancellationToken: token
            );
        }
    }

    private bool IsReplyToBot(Message message)
    {
        return message.ReplyToMessage != null
            && message.ReplyToMessage?.From.Id == 5759112130
            && threadService.IsUserMessageReplyToBot(
                message.Chat.Id,
                message.ReplyToMessage.MessageId
            );
    }

    private static string ExtractContentFromMessage(Message message)
    {
        return message.Text ?? message.Caption ?? string.Empty;
    }

    private static bool IsMediaMessage(Message message)
    {
        return message.Photo != null || message.Video != null;
    }

    private async Task<string> ProcessMediaMessageAsync(
        ITelegramBotClient botClient,
        Message message,
        string messageContent,
        CancellationToken token
    )
    {
        var mediaContent = await DownloadMedia(botClient, message, token);
        return await GenerateVisionResponse(mediaContent, messageContent);
    }

    private async Task<string> GenerateChatResponse(string threadKey)
    {
        try
        {
            var chatMessages = threadService.GetThreadHistory(threadKey);
            var configuration = chatConfigurationManager.CreateChatConfiguration();
            var response = await chatService.GenereateContentAsync(chatMessages, configuration);
            return response.Text();
        }
        catch (GeminiException ex)
        {
            logger.LogError(
                ex,
                "Error while generating chat response. Thread history: {FormattedThreadHistory}",
                GetFormattedThreadHistory(threadKey)
            );
            return string.Empty;
        }
    }

    private static async Task<string> DownloadMedia(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken token
    )
    {
        var fileId = message.Photo != null ? message.Photo[^1].FileId : message.Video.FileId;
        var fileInfo = await botClient.GetFileAsync(fileId, token);
        var stream = new MemoryStream();
        await botClient.DownloadFileAsync(fileInfo.FilePath, stream, token);
        stream.Position = 0;
        var mediaData = Convert.ToBase64String(stream.ToArray());
        return mediaData;
    }

    private string GetFormattedThreadHistory(string threadKey)
    {
        var threadHistory = threadService.GetThreadHistory(threadKey);
        return string.Join(
            Environment.NewLine,
            threadHistory.Select(m => $"Role: {m.Role}, Text: {m.Text}")
        );
    }

    private async Task<string> GenerateVisionResponse(string mediaContent, string messageContent)
    {
        var fileObject = new FileObject(Convert.FromBase64String(mediaContent), "image.jpg");
        var result = await visionService.GenereateContentAsync(messageContent, fileObject);
        return result.Text();
    }

    private static string ConvertToHtml(string markdown)
    {
        markdown = MyRegex().Replace(markdown, "$1&lt;$2&gt;");

        var html = Markdown.ToHtml(markdown);

        html = html.Replace("<p>", "")
            .Replace("</p>", "")
            .Replace("<ol>", "")
            .Replace("</ol>", "")
            .Replace("<ul>", "")
            .Replace("</ul>", "")
            .Replace("<li>", "• ")
            .Replace("</li>", "");

        return html;
    }

    [GeneratedRegex(@"(\w+)<(.+?)>")]
    private static partial Regex MyRegex();
}
