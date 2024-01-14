using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class RepeatedMessageStrategy(ITelegramBotClient botClient, IChatRepository chatRepository, IUserModerationService userModerationService ) : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        var chatSettings = await chatRepository.GetChatByIdAsync(message.Chat.Id, token);
        if (chatSettings is not { IsRepeatedMessageCheckEnabled: true }) return null;

        var messageContent = GetMessageContent(message);

        if (string.IsNullOrEmpty(messageContent)) return null;

        var user = await userModerationService.GetUserAsync(message.From.Id, message.Chat.Id, token);
        
        if (user.LastMessage == messageContent)
        {
            user.CountRepeatLastMessage++;
        }
        else
        {
            user.LastMessage = messageContent;
            user.CountRepeatLastMessage = 1;
        }

        if (user.CountRepeatLastMessage > 3)
        {
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken: token);
        }

        return null;
    }

    private static string GetMessageContent(Message message)
    {
        if (!string.IsNullOrEmpty(message.Text))
        {
            return message.Text.Length > 50 ? message.Text[..50] : message.Text;
        }

        if (message.Sticker != null)
        {
            return $"sticker_{message.Sticker.FileUniqueId}";
        }

        if (message.Animation != null)
        {
            return $"animation_{message.Animation.FileUniqueId}";
        }

        if (message.Photo is { Length: > 0 })
        {
            return $"photo_{message.Photo[^1].FileUniqueId}";
        }

        return message.Video != null ? $"video_{message.Video.FileUniqueId}" : string.Empty;
    }
}