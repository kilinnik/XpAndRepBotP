using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ChatRepository(IDbContextFactory<XpAndRepBotDbContext> contextFactory)
    : IChatRepository
{
    public async Task<Chat?> GetChatByIdAsync(long chatId, CancellationToken token)
    {
        await using var context = await contextFactory.CreateDbContextAsync(token);

        return await context
            .Chats.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ChatId == chatId, token);
    }

    public async Task UpdateChatSettingsAsync(Chat chat, CancellationToken token)
    {
        await using var context = await contextFactory.CreateDbContextAsync(token);
        context.Chats.Update(chat);
        await context.SaveChangesAsync(token);
    }
}
