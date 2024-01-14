using System.Text;
using Bll.Interfaces;
using Domain.Common;
using Domain.Models;
using Infrastructure.Interfaces;

namespace Bll.Services;

public class UserMarriageService(
    IUserMarriageRepository userMarriageRepository,
    IUserRepository userRepository
) : IUserMarriageService
{
    public async Task<UserMarriage> GetUserMarriageAsync(
        long userId,
        long chatId,
        CancellationToken cancellationToken
    )
    {
        return await userMarriageRepository.GetUserMarriageAsync(userId, chatId, cancellationToken);
    }

    public async Task<string> GetMarriagesAsync(long chatId, CancellationToken cancellationToken)
    {
        var marriedUsers = await userMarriageRepository.GetMarriedUsersWithPartnersAsync(
            chatId,
            cancellationToken
        );
        var sb = new StringBuilder("💒 Список браков: \n");
        var number = 1;

        foreach (var user in marriedUsers)
        {
            var partner = user.Partner;
            if (partner == null)
            {
                continue;
            }

            var ts = DateTime.UtcNow - user.MarriageDate;
            var formattedDate = user.MarriageDate.ToString("yy/MM/dd HH:mm:ss");

            sb.AppendLine(
                $"{Utils.NumberToEmoji(number++)} {user.FirstName} и {partner.FirstName} с {formattedDate} {ts.Days} дней, {ts.Hours} часов, {ts.Minutes} минут"
            );
        }

        return sb.ToString();
    }

    public async Task<string> GetMarriageStatusAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        var user = await userMarriageRepository.GetUserMarriageAsync(userId, chatId, token);
        if (user == null)
        {
            return "Пользователь не найден.";
        }

        if (user.PartnerId == 0)
        {
            return $"{user.FirstName} не состоит в браке.";
        }

        var partner = await userMarriageRepository.GetUserMarriageAsync(
            user.PartnerId,
            chatId,
            token
        );
        if (partner == null)
        {
            return "Партнер не найден.";
        }

        var timeSpan = DateTime.UtcNow - user.MarriageDate;
        return $"🤵🏿 🤵🏿 {user.FirstName} состоит в браке с {partner.FirstName} {timeSpan.Days} дней, {timeSpan.Hours} часов, {timeSpan.Minutes} минут. Дата регистрации: {user.MarriageDate:yy/MM/dd HH:mm:ss}";
    }

    public async Task<string> UpdateMarriageStatus(
        long chatId,
        long proposerId,
        long proposeeId,
        bool isAccepted,
        CancellationToken token
    )
    {
        if (!isAccepted)
        {
            return "Предложение брака отклонено.";
        }
        
        var proposerUser = await userRepository.GetUserAsync(proposerId, chatId, token);
    
        if (proposerId == proposeeId)
        {
            var selfMarriage = new UserMarriage(proposerId, chatId, proposerUser.FirstName, proposerId);
            await userMarriageRepository.CreateUserMarriageAsync(selfMarriage, token);

            selfMarriage.Partner = selfMarriage;
            await userMarriageRepository.UpdateUserMarriageAsync(selfMarriage, token);

            return $"💖 Поздравляем! {proposerUser.FirstName} теперь в браке с самим собой.";
        }

        var proposeeUser = await userRepository.GetUserAsync(proposeeId, chatId, token);
        var proposer = new UserMarriage(proposerId, chatId, proposerUser.FirstName, proposeeId);
        var proposee = new UserMarriage(proposeeId, chatId, proposeeUser.FirstName, proposerId);

        await userMarriageRepository.CreateUserMarriageAsync(proposer, token);
        await userMarriageRepository.CreateUserMarriageAsync(proposee, token);

        proposer = await userMarriageRepository.GetUserMarriageAsync(proposerId, chatId, token);
        proposee = await userMarriageRepository.GetUserMarriageAsync(proposeeId, chatId, token);

        proposer.Partner = proposee;
        proposee.Partner = proposer;

        await userMarriageRepository.UpdateUserMarriageAsync(proposer, token);
        await userMarriageRepository.UpdateUserMarriageAsync(proposee, token);

        return $"💖 Поздравляем! {proposer.FirstName} и {proposee.FirstName} теперь в браке.";
    }

    public async Task<string> ProcessDivorceAsync(
        long userId,
        long chatId,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await userMarriageRepository.BeginTransactionAsync(
            cancellationToken
        );

        try
        {
            var user = await userMarriageRepository.GetUserMarriageAsync(
                userId,
                chatId,
                cancellationToken
            );
            if (user == null)
            {
                return "Вы не состоите в браке.";
            }

            var partner = await userMarriageRepository.GetUserMarriageAsync(
                user.PartnerId,
                chatId,
                cancellationToken
            );

            await userMarriageRepository.RemoveUserMarriageAsync(user, cancellationToken);
            if (partner.UserId != user.UserId)
            {
                await userMarriageRepository.RemoveUserMarriageAsync(partner, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            var timeSpan = DateTime.UtcNow - partner.MarriageDate;
            return $"💔 {user.FirstName} сожалеем, но {partner.FirstName} подал на развод. Ваш брак просуществовал "
                + $"{timeSpan.Days} дней, {timeSpan.Hours} часов и {timeSpan.Minutes} минут.";
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            Console.WriteLine(e.Message, e.StackTrace);
            return string.Empty;
        }
    }

    public async Task<string?> CheckMarriageStatus(
        long proposerId,
        long proposeeId,
        long chatId,
        CancellationToken token
    )
    {
        var proposer = await userMarriageRepository.GetUserMarriageAsync(proposerId, chatId, token);
        var proposee = await userMarriageRepository.GetUserMarriageAsync(proposeeId, chatId, token);

        if (proposer == null && proposee == null)
        {
            return null;
        }

        return proposer.PartnerId != 0
            ? $"{proposer.FirstName} уже состоит в браке."
            : $"{proposee.FirstName} уже состоит в браке.";
    }
}
