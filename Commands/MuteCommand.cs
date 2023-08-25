using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XpAndRepBot.Database;

namespace XpAndRepBot.Commands;

public class MuteCommand : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.From == null) return;

        var chatMember =
            await botClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id, cancellationToken);
        await using var db = new DbUsersContext();
        var user = db.Users.FirstOrDefault(x => x.UserId == update.Message.From.Id && x.Roles.Contains("модер"));

        if (update.Message.ReplyToMessage?.From != null &&
            update.Message.From.Id != update.Message.ReplyToMessage.From.Id &&
            (user != null || (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator &&
                              update.Message.Chat.Id == Constants.NitokinChatId)))
        {
            var (days, hours, minutes) = ExtractMuteDuration(update.Message.Text);
            if (days == 0 && hours == 0 && minutes == 0)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Команда мута введена некорректно",
                    cancellationToken);
                return;
            }

            var muteDate = DateTime.Now.AddDays(days).AddHours(hours).AddMinutes(minutes);
            try
            {
                await botClient.RestrictChatMemberAsync(
                    chatId: update.Message.Chat.Id,
                    userId: update.Message.ReplyToMessage.From.Id,
                    new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false },
                    untilDate: muteDate,
                    cancellationToken: cancellationToken);

                var userMute = db.Users.FirstOrDefault(x => x.UserId == update.Message.ReplyToMessage.From.Id);
                if (userMute != null) userMute.DateMute = muteDate;
                await db.SaveChangesAsync(cancellationToken);

                var durationText = GetFormattedDuration(days, hours, minutes);
                var messageText =
                    $"{update.Message.ReplyToMessage.From.FirstName} получил мут на {durationText} до {muteDate:dd.MM.yyyy HH:mm}";

                await botClient.SendTextMessageAsync(update.Message.Chat.Id, messageText, cancellationToken,
                    update.Message.ReplyToMessage.MessageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        else
        {
            try
            {
                await botClient.DeleteMessageAsync(update.Message, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private static (int days, int hours, int minutes) ExtractMuteDuration(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= 6) return (0, 0, 0);

        var parts = text[6..].Split(' ');
        int days = 0, hours = 0, minutes = 0;
        foreach (var part in parts)
        {
            if (part.EndsWith("d") && int.TryParse(part.TrimEnd('d'), out var resultDays)) days = resultDays;
            else if (part.EndsWith("h") && int.TryParse(part.TrimEnd('h'), out var resultHours)) hours = resultHours;
            else if (part.EndsWith("m") && int.TryParse(part.TrimEnd('m'), out var resultMinutes))
                minutes = resultMinutes;
        }

        return (days, hours, minutes);
    }

    private static string GetFormattedDuration(int days, int hours, int minutes)
    {
        var duration = "";

        if (days > 0) duration += $"{days} {GetNounForm(days, "день", "дня", "дней")} ";
        if (hours > 0) duration += $"{hours} {GetNounForm(hours, "час", "часа", "часов")} ";
        if (minutes > 0) duration += $"{minutes} {GetNounForm(minutes, "минуту", "минуты", "минут")} ";

        return duration.Trim();
    }

    private static string GetNounForm(int number, string form1, string form2, string form5)
    {
        var mod10 = number % 10;
        var mod100 = number % 100;
        if (mod100 is >= 11 and <= 19)
            return form5;
        return mod10 switch
        {
            1 => form1,
            2 or 3 or 4 => form2,
            _ => form5,
        };
    }
}