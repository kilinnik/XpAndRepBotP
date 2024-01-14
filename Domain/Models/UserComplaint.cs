namespace Domain.Models;

public class UserComplaint(long userId, long chatId, string firstName, string complaintList, string complainerList)
{
    public long UserId { get; init; } = userId;
    public long ChatId { get; init; } = chatId;
    public string FirstName { get; set; } = firstName;
    public string ComplaintList { get; set; } = complaintList;
    public string ComplainerList { get; set; } = complainerList;
}