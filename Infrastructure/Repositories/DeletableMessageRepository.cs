using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DeletableMessageRepository(IDbContextFactory<XpAndRepBotDbContext> contextFactory)
    : IDeletableMessageRepository
{
    private readonly XpAndRepBotDbContext _context = contextFactory.CreateDbContext();

    public async Task<bool> ContainsMessageIdAsync(
        long messageId,
        long chatId,
        CancellationToken token
    )
    {
        return await _context.DeletableMessages.AnyAsync(
            dm => dm.ChatId == chatId && dm.MessageIds.Contains(messageId.ToString()),
            token
        );
    }

    public async Task AddAsync(DeletableMessage deletableMessage, CancellationToken token)
    {
        _context.DeletableMessages.Add(deletableMessage);
        await _context.SaveChangesAsync(token);
    }

    public async Task AppendMessageIdAsync(
        long originalMessageId,
        long newMessageId,
        long chatId,
        CancellationToken token
    )
    {
        var deletableMessage = await _context.DeletableMessages.FirstOrDefaultAsync(
            dm => dm.ChatId == chatId && dm.MessageIds.Contains(originalMessageId.ToString()),
            token
        );

        if (deletableMessage != null)
        {
            deletableMessage.MessageIds += " " + newMessageId;
            await _context.SaveChangesAsync(token);
        }
    }
}
