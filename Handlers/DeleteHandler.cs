using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public static class DeleteHandler
{
    private static async Task<bool> ContainsMessageId(long targetMessageId)
    {
        await using var db = new DbUsersContext();
        return await db.MessageIdsForDeletion.AnyAsync(row => row.MessageIds.Contains(targetMessageId.ToString()));
    }

    public static async Task DeleteUnwantedMessages(ITelegramBotClient botClient, Update update, Users user,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = update.Message;
            if (message == null) return;

            await using var db = new DbUsersContext();

            HandleServiceMessage(db, message, user, cancellationToken);
            HandleForbiddenWords(botClient, message, cancellationToken);
            HandleMediaRestrictions(botClient, message, user, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static async void HandleServiceMessage(DbUsersContext db, Message message, Users user,
        CancellationToken cancellationToken)
    {
        if (message.From?.Id == 777000)
        {
            db.MessageIdsForDeletion.Add(new MessageIdsForDelete(message.MessageId, message.MessageId.ToString(),
                user.ChatId));
            await db.SaveChangesAsync(cancellationToken);
        }

        if (message.ReplyToMessage == null ||
            !await ContainsMessageId(message.ReplyToMessage.MessageId)) return;
        db = new DbUsersContext();
        var rowToUpdate = db.MessageIdsForDeletion.FirstOrDefault(row =>
            row.MessageIds.Contains(message.ReplyToMessage.MessageId.ToString()));
        if (rowToUpdate == null) return;
        rowToUpdate.MessageIds += " " + message.MessageId;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async void HandleForbiddenWords(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var flagForDelete = message.ReplyToMessage != null &&
                            await ContainsMessageId(message.ReplyToMessage.MessageId);
        var containsForbiddenWords = flagForDelete && Constants.ForbiddenWords.Any(s =>
            !string.IsNullOrEmpty(message.Text) && message.Text.ToLower().Contains(s));

        if (containsForbiddenWords)
        {
            await botClient.DeleteMessageAsync(message, cancellationToken);
        }
    }

    private static async void HandleMediaRestrictions(ITelegramBotClient botClient, Message message, Users user,
        CancellationToken cancellationToken)
    {
        var flagForDelete = message.ReplyToMessage != null &&
                            await ContainsMessageId(message.ReplyToMessage.MessageId);
        var flag = message.MessageId % 10 == 0;

        var restrictions = new List<(Func<Message, bool> Condition, int Level, string Text)>
        {
            (m => m.Animation != null, 10, "Гифки с 10 лвла и в ответ кидать нельзя.\n/m - посмотреть свой лвл"),
            (m => m.Sticker != null, 15, "Стикеры с 15 лвла и в ответ кидать нельзя.\n/m - посмотреть свой лвл"),
            (m => m.Poll != null, 20, "Опросы с 20 лвла.\n/m - посмотреть свой лвл"),
            (m => m.Video != null || m.Voice != null || m.VideoNote != null || m.Document != null || m.Audio != null,
                0, null)
        };

        foreach (var (condition, level, text) in restrictions)
        {
            if (!condition(message)) continue;

            if (user.Lvl >= level && !flagForDelete) continue;
            if (!flagForDelete && flag && !string.IsNullOrEmpty(text))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, text,
                    cancellationToken: cancellationToken);
            }

            await botClient.DeleteMessageAsync(message, cancellationToken);
            return;
        }
    }
}