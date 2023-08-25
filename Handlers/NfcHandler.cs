using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;
using XpAndRepBot.Database.Models;

namespace XpAndRepBot.Handlers;

public static class NfcHandler
{
    public static async Task HandleNfc(ITelegramBotClient botClient, Update update, Users user,
        CancellationToken cancellationToken)
    {
        var message = update.Message;
        if (message == null) return;

        await using var db = new DbUsersContext();
        if (update.Message != null && message.From is { Id: 777000 } && update.Message.Chat.Id == Constants.IgruhaChatId)
        {
            db.MessageIdsForDeletion.Add(new MessageIdsForDelete(message.MessageId,
                message.MessageId.ToString(), user.ChatId));
            await db.SaveChangesAsync(cancellationToken);
        }

        var flagForDelete = message.ReplyToMessage != null &&
                            Utilities.ContainsMessageId(db, message.ReplyToMessage.MessageId);
        if (flagForDelete)
        {
            var rowToUpdate = db.MessageIdsForDeletion.FirstOrDefault(row =>
                row.MessageIds.Contains(message.ReplyToMessage.MessageId.ToString()));
            if (rowToUpdate != null)
            {
                rowToUpdate.MessageIds += " " + message.MessageId;
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        if ((user.Nfc == true || flagForDelete) && update.Message != null)
        {
            var words = await ReadResourceLinesToLower("XpAndRepBot.bw.txt");
            var messageWords = Utilities.GetMessageText(update).ToLower()
                .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var containsWord = messageWords.Any(w => words.Contains(w));
            if (containsWord)
            {
                try
                {
                    if (user.Nfc == true)
                    {
                        await botClient.SendTextMessageAsync(user.ChatId, await Nfc(user, botClient, cancellationToken),
                            cancellationToken);
                    }

                    if (flagForDelete)
                    {
                        await botClient.DeleteMessageAsync(update.Message, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }

    private static async Task<string> Nfc(Users user, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        await using var db = new DbUsersContext();

        var chatMember = await botClient.GetChatMemberAsync(user.ChatId, user.UserId, cancellationToken);

        user.Nfc = false;
        var ts = DateTime.Now - user.StartNfc;
        if (ts.Ticks > user.BestTime)
            user.BestTime = ts.Ticks;

        await db.SaveChangesAsync(cancellationToken);

        var t = $"{ts.Days} d, {ts.Hours} h, {ts.Minutes} m.";
        var muteDate = DateTime.Now.AddMinutes(15);

        if (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator)
            return $"{user.Name} нарушил условия no fuck challenge. Время: {t}";
        try
        {
            await botClient.RestrictChatMemberAsync(
                chatId: user.ChatId,
                userId: user.UserId,
                new ChatPermissions
                {
                    CanSendMessages = false,
                    CanSendMediaMessages = false
                },
                untilDate: muteDate,
                cancellationToken: cancellationToken
            );

            await botClient.SendTextMessageAsync(user.ChatId,
                $"{user.Name} получил мут на 15 минут до {muteDate:dd.MM.yyyy HH:mm}", cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return $"{user.Name} нарушил условия no fuck challenge. Время: {t}";
    }

    private static async Task<List<string>> ReadResourceLinesToLower(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new Exception($"Resource {resourceName} not found.");

        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            lines.Add(await reader.ReadLineAsync());
        }

        return lines.Select(line => line.ToLower()).ToList();
    }
}