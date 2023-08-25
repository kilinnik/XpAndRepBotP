using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;

namespace XpAndRepBot.Handlers;

public static class GetChatPermissionsHandler
{
    public static async Task<bool> GetChatPermissions(string option, CallbackQuery callbackQuery,
        ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var userId = long.Parse(option[2..]);
        if (callbackQuery.From.Id != userId) return false;

        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(x => x.UserId == userId && x.ChatId == chatId);
        var chat = db.Chats.FirstOrDefault(x => x.ChatId == chatId);

        if (user == null) return false;

        if (option.Contains('y') && !user.CheckEnter)
        {
            await GrantChatPermissions(botClient, chatId, userId, cancellationToken);
            if (chat != null)
            {
                var welcomeMessage = $"Привет, {callbackQuery.From.FirstName}.{chat.Greeting}";
                await botClient.SendTextMessageAsync(chatId, welcomeMessage, cancellationToken);
            }

            user.CheckEnter = true;
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await botClient.BanChatMemberAsync(chatId: chatId, userId: userId,
                cancellationToken: cancellationToken);
        }

        return true;
    }

    private static async Task GrantChatPermissions(ITelegramBotClient botClient, long chatId, long userId,
        CancellationToken cancellationToken)
    {
        var permissions = new ChatPermissions
        {
            CanSendMessages = true,
            CanSendMediaMessages = true,
            CanSendOtherMessages = true,
            CanSendPolls = true,
            CanAddWebPagePreviews = true,
            CanChangeInfo = true,
            CanInviteUsers = true,
            CanPinMessages = true
        };
        await botClient.RestrictChatMemberAsync(chatId: chatId, userId, permissions,
            cancellationToken: cancellationToken);
    }
}