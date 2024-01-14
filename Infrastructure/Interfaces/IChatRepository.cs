using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IChatRepository
{
    Task<Chat?> GetChatByIdAsync(long chatId, CancellationToken token);
    
    Task UpdateChatSettingsAsync(Chat chat, CancellationToken token);
}
