using Bll.Interfaces;
using Bll.Services;
using Domain.DTO;
using Domain.Models;
using Infrastructure.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class ContentModerationStrategy(
    IChatRepository chatRepository,
    WordListService wordListService,
    IUserLevelRepository userLevelRepository,
    IUserNfcService userNfcService,
    IDeletableMessageRepository deletableMessageRepository,
    ITelegramBotClient botClient
) : IUserMessageStrategy
{
    private readonly Random _random = new();
    private static readonly char[] Separator = [' ', ',', '.', '!', '?'];

    public async Task<CommandResult?> ExecuteAsync(
        Message message,
        CancellationToken token
    )
    {
        var levelAsync = await userLevelRepository.GetUserLevelAsync(message.From.Id, message.Chat.Id, token);

        if (await IsRelevantChat(levelAsync, token))
        {
            return null;
        }

        await HandleSpecialMessagesIfAny(message, token);

        if (await ShouldMessageBeDeleted(message, token))
        {
            return await HandleMessageDeletion(message, token);
        }

        return await HandleLevelRestrictions(levelAsync, message, token);
    }

    private async Task<bool> IsRelevantChat(UserLevel user, CancellationToken token)
    {
        var chatSettings = await chatRepository.GetChatByIdAsync(user.ChatId, token);
        return chatSettings is not { IsContentModerationEnabled: true };
    }

    private async Task HandleSpecialMessagesIfAny(Message message, CancellationToken token)
    {
        var isSpecialMessage = message.From?.Id == 777000;
        if (isSpecialMessage || message.ReplyToMessage != null)
        {
            await HandleSpecialMessageAsync(message, token);
        }
    }

    private async Task<bool> ShouldMessageBeDeleted(Message message, CancellationToken token)
    {
        return IsMessageInappropriate(message) && await IsReplyToDelete(message, token);
    }

    private async Task<bool> IsReplyToDelete(Message message, CancellationToken token)
    {
        return message.ReplyToMessage != null
               && await deletableMessageRepository.ContainsMessageIdAsync(
                   message.ReplyToMessage.MessageId,
                   message.Chat.Id,
                   token
               );
    }

    private async Task<CommandResult?> HandleMessageDeletion(
        Message message,
        CancellationToken token
    )
    {
        var user = await userNfcService.GetUserNfcAsync(message.From.Id, message.Chat.Id, token);
        
        if (user.IsNfcActive)
        {
            var chatMember = await botClient.GetChatMemberAsync(
                user.ChatId,
                user.UserId,
                cancellationToken: token
            );
            var (violationMessage, muteMessage) =
                await userNfcService.EvaluateNfcViolationAsync(
                    chatMember.Status,
                    user,
                    message,
                    token
                );
            return new CommandResult(
                message.Chat.Id,
                new List<string> { violationMessage, muteMessage }
            );
        }

        await botClient.DeleteMessageAsync(
            message.Chat.Id,
            message.MessageId,
            cancellationToken: token
        );

        return null;
    }

    private async Task<CommandResult?> HandleLevelRestrictions(
        UserLevel user,
        Message message,
        CancellationToken token
    )
    {
        var shouldSendWarning = _random.Next(10) == 0;

        var isReplyToDelete = await IsReplyToDelete(message, token);

        foreach (var (condition, requiredLevel, warningText) in GetLevelRestrictions())
        {
            if (!condition(message) || user.Level >= requiredLevel && !isReplyToDelete)
                continue;
            if (shouldSendWarning && !string.IsNullOrEmpty(warningText) && !isReplyToDelete)
            {
                await botClient.SendTextMessageAsync(
                    user.ChatId,
                    warningText,
                    cancellationToken: token
                );

                await botClient.DeleteMessageAsync(
                    message.Chat.Id,
                    message.MessageId,
                    cancellationToken: token
                );

                return new CommandResult(message.Chat.Id, new List<string> { warningText });
            }

            await botClient.DeleteMessageAsync(
                message.Chat.Id,
                message.MessageId,
                cancellationToken: token
            );
            return null;
        }

        return null;
    }

    private bool IsMessageInappropriate(Message message)
    {
        var text = message.Text ?? message.Caption;
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        text = text.ToLower();
        var messageWords = text.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        return messageWords.Any(word => wordListService.BannedWords.Contains(word))
               || wordListService.ForbiddenPhrases.Any(phrase => text.Contains(phrase));
    }

    private static List<(Func<Message, bool>, int, string)> GetLevelRestrictions()
    {
        return
        [
            (
                m => m.Animation != null,
                10,
                "Гифки с 10 лвла и в ответ кидать нельзя.\n/m - посмотреть свой уровень"
            ),
            (
                m => m.Sticker != null,
                15,
                "Стикеры с 15 лвла и в ответ кидать нельзя.\n/m - посмотреть свой уровень"
            ),
            (m => m.Poll != null, 20, "Опросы с 20 лвла.\n/m - посмотреть свой уровень"),
            (
                m => m.Video != null || m.Voice != null || m.VideoNote != null || m.Audio != null,
                0,
                null
            )!
        ];
    }

    private async Task HandleSpecialMessageAsync(Message message, CancellationToken token)
    {
        if (message.From?.Id == 777000)
        {
            var newDeletableMessage = new DeletableMessage(
                message.MessageId,
                message.MessageId.ToString(),
                message.Chat.Id
            );
            await deletableMessageRepository.AddAsync(newDeletableMessage, token);
        }
        else if (message.ReplyToMessage != null)
        {
            await UpdateDeletableMessageAsync(message, token);
        }
    }

    private async Task UpdateDeletableMessageAsync(Message message, CancellationToken token)
    {
        if (
            await deletableMessageRepository.ContainsMessageIdAsync(
                message.ReplyToMessage.MessageId,
                message.Chat.Id,
                token
            )
        )
        {
            await deletableMessageRepository.AppendMessageIdAsync(
                message.ReplyToMessage.MessageId,
                message.MessageId,
                message.Chat.Id,
                token
            );
        }
    }
}