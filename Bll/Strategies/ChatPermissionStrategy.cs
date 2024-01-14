using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bll.Strategies;

public class ChatPermissionStrategy(ITelegramBotClient botClient, IChatRepository chatRepository)
    : IUserMessageStrategy
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        var chatSettings = await chatRepository.GetChatByIdAsync(message.Chat.Id, token);

        if (chatSettings is not { IsRestrictionEnabled: true } || message.From.Id != 777000) return null;

        await SetChatPermissionsAsync(botClient, message.Chat.Id);
        return null;
    }

    private static async Task SetChatPermissionsAsync(ITelegramBotClient botClient, long chatId)
    {
        await botClient.SetChatPermissionsAsync(
            chatId,
            new ChatPermissions
            {
                CanSendMessages = true,
                CanSendAudios = true,
                CanSendDocuments = true,
                CanSendPhotos = true,
                CanSendPolls = true,
                CanSendVideoNotes = true,
                CanSendVideos = true,
                CanSendVoiceNotes = true,
                CanAddWebPagePreviews = true,
                CanSendOtherMessages = false,
                CanInviteUsers = false,
                CanChangeInfo = false,
                CanPinMessages = false,
                CanManageTopics = false
            }
        );
    }
}
