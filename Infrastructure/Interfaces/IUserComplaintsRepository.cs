namespace Infrastructure.Interfaces;

public interface IUserComplaintsRepository
{
    Task<bool> AddUserComplaintAsync(
        long complainantUserId,
        long reportedUserId,
        string complaintText,
        long chatId,
        CancellationToken token
    );

    Task<(string ComplaintList, string UserName)> GetUserComplaintsAsync(
        long userId,
        long chatId,
        CancellationToken token
    );
}
