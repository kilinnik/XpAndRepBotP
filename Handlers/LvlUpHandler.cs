using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public static class LvlUpHandler
{
    public static async Task HandleLevelUp(ITelegramBotClient botClient, Update update, DbContext db, Users user,
        CancellationToken cancellationToken)
    {
        if (user.ChatId != Constants.Mid)
        {
            await LvlUp(botClient, update, db, user, cancellationToken);
        }
    }

    private static async Task LvlUp(ITelegramBotClient botClient, Update update, DbContext db, Users user,
        CancellationToken cancellationToken)
    {
        while (user.CurXp >= Utilities.GenerateXpForLevel(user.Lvl + 1))
        {
            LevelUpUser(user);
            if (update.Message == null) continue;
            await NotifyLevelUp(botClient, user, cancellationToken);
            if (ShouldRestrictUser(user, update))
            {
                await RestrictUser(botClient, user, update, cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void LevelUpUser(Users user)
    {
        user.Lvl++;
        user.CurXp -= Utilities.GenerateXpForLevel(user.Lvl);
    }

    private static async Task NotifyLevelUp(ITelegramBotClient botClient, Users user,
        CancellationToken cancellationToken)
    {
        var message = $"{user.Name} получает {user.Lvl} lvl";
        await botClient.SendTextMessageAsync(user.ChatId, message, cancellationToken);
    }

    private static bool ShouldRestrictUser(Users user, Update update)
    {
        return user.Lvl == 10 && user.ChatId == Constants.IgruhaChatId && update.Message?.From != null;
    }

    private static async Task RestrictUser(ITelegramBotClient botClient, Users user, Update update,
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
        if (update.Message is { From: not null })
        {
            await botClient.RestrictChatMemberAsync(
                user.ChatId,
                update.Message.From.Id,
                permissions,
                cancellationToken: cancellationToken);
        }
    }
}