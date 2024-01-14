using Bll.Interfaces;
using Domain.Common;
using Domain.DTO;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bll.Commands.AdminCommands;

public class MuteUserCommand(
    ITelegramBotClient botClient,
    IUserRoleService userRoleService,
    IUserModerationService userModerationService,
    ILogger<MuteUserCommand> logger
) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var executingUser = await userRoleService.GetUserRoleAsync(
                message.From.Id,
                message.Chat.Id,
                token
            );
            if (!Utils.IsUserModerator(executingUser.Roles))
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, token);
                return null;
            }

            if (message.ReplyToMessage == null || message.ReplyToMessage.From.IsBot)
            {
                return new CommandResult(
                    message.Chat.Id,
                    new List<string> { Resources.ReplyToMsg },
                    message.MessageId
                );
            }

            var targetUserId = message.ReplyToMessage.From.Id;
            var targetChatId = message.Chat.Id;

            var chatMember = await botClient.GetChatMemberAsync(targetChatId, targetUserId, token);
            if (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator)
            {
                return new CommandResult(
                    message.Chat.Id,
                    new List<string> { Resources.UserAdmin },
                    message.MessageId
                );
            }

            var (days, hours, minutes) = ExtractMuteDuration(message.Text);
            if (days == 0 && hours == 0 && minutes == 0)
            {
                return new CommandResult(
                    message.Chat.Id,
                    new List<string> { "Команда введена некорректно" },
                    message.MessageId
                );
            }

            var muteDuration = new TimeSpan(days, hours, minutes, 0);

            await userModerationService.MuteUserAsync(
                message.ReplyToMessage.From.Id,
                message.Chat.Id,
                muteDuration,
                token
            );

            var formattedDuration = GetFormattedDuration(days, hours, minutes);
            var muteMessage =
                $"{message.ReplyToMessage.From.FirstName} получил мут на {formattedDuration}";

            return new CommandResult(
                message.Chat.Id,
                new List<string> { muteMessage },
                message.ReplyToMessage.MessageId
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in MuteUserCommand");
            return null;
        }
    }

    private static (int days, int hours, int minutes) ExtractMuteDuration(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= 6)
            return (0, 0, 0);

        var parts = text[6..].Split(' ');
        int days = 0,
            hours = 0,
            minutes = 0;
        foreach (var part in parts)
        {
            if (part.EndsWith('d') && int.TryParse(part.TrimEnd('d'), out var resultDays))
                days = resultDays;
            else if (part.EndsWith('h') && int.TryParse(part.TrimEnd('h'), out var resultHours))
                hours = resultHours;
            else if (part.EndsWith('m') && int.TryParse(part.TrimEnd('m'), out var resultMinutes))
                minutes = resultMinutes;
        }

        return (days, hours, minutes);
    }

    private static string GetFormattedDuration(int days, int hours, int minutes)
    {
        var duration = "";

        if (days > 0)
            duration += $"{days} {GetNounForm(days, "день", "дня", "дней")} ";
        if (hours > 0)
            duration += $"{hours} {GetNounForm(hours, "час", "часа", "часов")} ";
        if (minutes > 0)
            duration += $"{minutes} {GetNounForm(minutes, "минуту", "минуты", "минут")} ";

        return duration.Trim();
    }

    private static string GetNounForm(int number, string form1, string form2, string form5)
    {
        var mod10 = number % 10;
        var mod100 = number % 100;

        if (mod100 is >= 11 and <= 19)
        {
            return form5;
        }

        return mod10 switch
        {
            1 => form1,
            2 or 3 or 4 => form2,
            _ => form5
        };
    }
}
