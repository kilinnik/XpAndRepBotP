using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserComplaintsRepository(XpAndRepBotDbContext context, IUserRepository userRepository)
    : IUserComplaintsRepository
{
    public async Task<bool> AddUserComplaintAsync(
        long complainantUserId,
        long reportedUserId,
        string complaintText,
        long chatId,
        CancellationToken token
    )
    {
        var complainantUser = await userRepository.GetUserAsync(reportedUserId, chatId, token);
        var reportedUser = await context.UserComplaints.FirstOrDefaultAsync(
            u => u.UserId == complainantUserId && u.ChatId == chatId,
            cancellationToken: token
        );

        if (reportedUser == null)
        {
            var newComplaintUser = new UserComplaint(
                userId: complainantUserId,
                chatId: chatId,
                firstName: complainantUser.FirstName,
                complaintList: $"{complaintText} ({complainantUser.FirstName})\n",
                complainerList: $"{complainantUserId},"
            );

            context.UserComplaints.Add(newComplaintUser);
        }
        else
        {
            if (reportedUser.ComplainerList.Contains(complainantUserId.ToString()))
            {
                return false;
            }

            reportedUser.ComplainerList += $"{complainantUserId},";
            reportedUser.ComplaintList += $"{complaintText} ({complainantUser.FirstName})\n";
        }

        await context.SaveChangesAsync(token);
        return true;
    }

    public async Task<(string ComplaintList, string UserName)> GetUserComplaintsAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        var user = await context
            .UserComplaints.AsNoTracking()
            .Where(u => u.UserId == userId && u.ChatId == chatId)
            .Select(u => new { u.ComplaintList, Name = u.FirstName })
            .FirstOrDefaultAsync(token);

        return (user?.ComplaintList ?? string.Empty, user?.Name ?? "Неизвестный пользователь");
    }
}
