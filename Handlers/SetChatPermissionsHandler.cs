using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace XpAndRepBot.Handlers;

public static class SetChatPermissionsHandler
{
    public static async Task SetChatPermissionsAsync(ITelegramBotClient botClient, long chatId)
    {
        try
        {
            await botClient.SetChatPermissionsAsync(chatId, new ChatPermissions
            {
                CanSendMessages = true,
                CanSendMediaMessages = false, // Members can't send photos, videos, etc.
                CanSendOtherMessages = false, // Members can't send stickers, GIFs, etc.
                CanAddWebPagePreviews = false, // Members can't add web page previews
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while setting chat permissions for chat {chatId}: {ex.Message}");
        }
    }
}