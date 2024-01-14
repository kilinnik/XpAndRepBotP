using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IDeletableMessageRepository
{
    Task<bool> ContainsMessageIdAsync(long messageId, long chatId, CancellationToken token);

    Task AddAsync(DeletableMessage deletableMessage, CancellationToken token);

    Task AppendMessageIdAsync(
        long originalMessageId,
        long newMessageId,
        long chatId,
        CancellationToken token
    );
}
