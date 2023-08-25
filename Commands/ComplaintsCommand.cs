using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class ComplaintsCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        var (userId, replyId) = await ExtractUserIdAndReplyId(update);

        if (userId == 0) return;

        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(x => x.UserId == userId && x.ChatId == update.Message.Chat.Id);

        if (user == null) return;

        try
        {
            await botClient.SendTextMessageAsync(user.ChatId, $"Список жалоб на {user.Name}:\n{user.Complaints}",
                cancellationToken, replyId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static async Task<(long userId, int replyId)> ExtractUserIdAndReplyId(Update update)
    {
        long userId = 0;
        var replyId = -1;

        switch (update.Message)
        {
            case { Entities: not null } when update.Message.Entities.Any(x =>
                x.Type is MessageEntityType.TextMention or MessageEntityType.Mention):
                userId = await ExtractUserIdFromEntities(update);
                break;
            case { ReplyToMessage.From.IsBot: false } when update.Message.ReplyToMessage.From.Id != 777000:
                userId = update.Message.ReplyToMessage.From.Id;
                replyId = update.Message.ReplyToMessage.MessageId;
                break;
            case { From: not null }:
                userId = update.Message.From.Id;
                replyId = update.Message.MessageId;
                break;
        }

        return (userId, replyId);
    }

    private static async Task<long> ExtractUserIdFromEntities(Update update)
    {
        await using var db = new DbUsersContext();

        if (update.Message is not { Entities: not null }) return 0;
        foreach (var entity in update.Message.Entities)
        {
            switch (entity.Type)
            {
                case MessageEntityType.TextMention:
                    return entity.User?.Id ?? 0;
                case MessageEntityType.Mention:
                    if (update.Message.Text == null) continue;
                    var username = update.Message.Text.Substring(entity.Offset + 1, entity.Length - 1);
                    var user = db.Users.FirstOrDefault(x =>
                        x.ChatId == update.Message.Chat.Id && x.Username == username);
                    return user?.UserId ?? 0;
            }
        }

        return 0;
    }
}