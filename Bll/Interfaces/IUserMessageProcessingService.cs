using Domain.Models;
using Telegram.Bot.Types;

namespace Bll.Interfaces;

public interface IUserMessageProcessingService
{
    Task ProcessUserMessageAsync(Message message, ChatUser chatUser, CancellationToken token);
}