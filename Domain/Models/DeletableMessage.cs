namespace Domain.Models;

public class DeletableMessage(long firstMessageId, string messageIds, long chatId)
{
    public long FirstMessageId { get; init; } = firstMessageId;
    public string MessageIds { get; set; } = messageIds;
    public long ChatId { get; init; } = chatId;
}
