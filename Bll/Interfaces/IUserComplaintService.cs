namespace Bll.Interfaces;

public interface IUserComplaintService
{
    Task<(string ComplaintList, string UserName)> GetUserComplaintsAsync(long userId, long chatId,
        CancellationToken token);

    Task<bool> ReportUserAsync(long complainantUserId, long reportedUserId, string complaintText, long chatId,
        CancellationToken token);
}