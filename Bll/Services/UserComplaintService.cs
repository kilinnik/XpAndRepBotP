using Bll.Interfaces;
using Infrastructure.Interfaces;

namespace Bll.Services;

public class UserComplaintService(IUserComplaintsRepository userComplaintsRepository)
    : IUserComplaintService
{
    public async Task<(string ComplaintList, string UserName)> GetUserComplaintsAsync(
        long userId,
        long chatId,
        CancellationToken token
    )
    {
        return await userComplaintsRepository.GetUserComplaintsAsync(userId, chatId, token);
    }

    public async Task<bool> ReportUserAsync(
        long complainantUserId,
        long reportedUserId,
        string complaintText,
        long chatId,
        CancellationToken token
    )
    {
        return await userComplaintsRepository.AddUserComplaintAsync(
            complainantUserId,
            reportedUserId,
            complaintText,
            chatId,
            token
        );
    }
}
