using System.Text;
using Bll.Interfaces;
using Domain.Common;
using Domain.Models;
using Infrastructure.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bll.Services;

public class UserNfcService(
    IUserNfcRepository userNfcRepository,
    WordListService wordListService,
    IUserModerationService userModerationService
) : IUserNfcService
{
    private static readonly char[] Separator = [' ', ',', '.', '!', '?'];

    public Task<UserNfc> GetUserNfcAsync(long userId, long chatId, CancellationToken token)
    {
        return userNfcRepository.GetUserAsync(userId, chatId, token);
    }

    public async Task<string> StartOrCheckNfcAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        var user = await userNfcRepository.GetUserAsync(userId, chatId, token);

        if (user.IsNfcActive)
        {
            return await GetCurrentNfcStatusAsync(chatId, token);
        }

        user.IsNfcActive = true;
        user.StartNfcDate = DateTime.UtcNow;

        await userNfcRepository.UpdateUserAsync(user, token);

        var bestTimeMessage = GenerateBestTimeMessage(user.NfcBestTime);
        return $"Вы начали новую серию без мата 👮‍♂️{bestTimeMessage}\nУдачи 😉";
    }

    public async Task<(string, string)> EvaluateNfcViolationAsync(
        ChatMemberStatus status,
        UserNfc? userNfc,
        Message message,
        CancellationToken token
    )
    {
        var text = message.Text ?? message.Caption;
        if (text == null)
        {
            return (string.Empty, string.Empty);
        }

        var messageWords = text.ToLower().Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        var containsBadWord = messageWords.Any(w => wordListService.BannedWords.Contains(w));

        if (containsBadWord && userNfc is not null && userNfc.IsNfcActive)
        {
            return await ResetNfcAsync(status, userNfc, message.Chat.Id, token);
        }

        return (string.Empty, string.Empty);
    }

    private async Task<string> GetCurrentNfcStatusAsync(
        long chatId,
        CancellationToken cancellationToken
    )
    {
        var usersWithNfc = await userNfcRepository.GetUsersWithActiveNfcAsync(
            chatId,
            cancellationToken
        );

        var sb = new StringBuilder("Без мата 👮‍♂️\n");
        for (var i = 0; i < usersWithNfc.Count; i++)
        {
            var ts = DateTime.UtcNow - usersWithNfc[i].StartNfcDate;
            sb.AppendLine(
                $"{Utils.NumberToEmoji(i + 1)} || {usersWithNfc[i].FirstName}: {ts.Days} d, {ts.Hours} h, {ts.Minutes} m."
            );
        }

        return sb.ToString();
    }

    private async Task<(string, string)> ResetNfcAsync(
        ChatMemberStatus status,
        UserNfc user,
        long chatId,
        CancellationToken token
    )
    {
        user.IsNfcActive = false;
        var ts = DateTime.UtcNow - user.StartNfcDate;
        if (ts.Ticks > user.NfcBestTime)
        {
            user.NfcBestTime = ts.Ticks;
        }

        var challengeFailedMessage =
            $"{user.FirstName} нарушил условия NFC. Время: {ts.Days}d, {ts.Hours}h, {ts.Minutes}m.";
        var muteMessage = string.Empty;

        if (
            status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator
            || chatId == user.UserId
        )
        {
            return (challengeFailedMessage, muteMessage);
        }

        await userModerationService.MuteUserAsync(
            user.UserId,
            chatId,
            new TimeSpan(0, 15, 0),
            token
        );
        muteMessage = $"{user.FirstName} получил мут на 15 минут";

        return (challengeFailedMessage, muteMessage);
    }

    private static string GenerateBestTimeMessage(long nfcBestTime)
    {
        if (nfcBestTime <= 0)
        {
            return string.Empty;
        }

        var ts = TimeSpan.FromTicks(nfcBestTime);
        return $"\nВаш лучший результат: {ts.Days} d, {ts.Hours} h, {ts.Minutes} m.";
    }
}
