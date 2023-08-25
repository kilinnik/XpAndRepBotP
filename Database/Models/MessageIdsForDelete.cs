namespace XpAndRepBot.Database.Models;

public class MessageIdsForDelete
{
    public long FirstMessageId { get; set; }
    public string MessageIds { get; set; }
    public long ChatId { get; set; }
    public Chat Chat { get; set; }

    public MessageIdsForDelete(long firstMessageId, string messageIds, long chatId)
    {
        FirstMessageId = firstMessageId;
        MessageIds = messageIds;
        ChatId = chatId;
    }
}