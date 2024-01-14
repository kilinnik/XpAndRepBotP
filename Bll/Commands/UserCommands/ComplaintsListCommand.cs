using Bll.Interfaces;
using Domain.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bll.Commands.UserCommands;

public class ComplaintsListCommand(
    IUserRepository userRepository,
    IUserComplaintService userComplaintService, 
    ILogger<ComplaintsListCommand> logger) : ICommand
{
    public async Task<CommandResult?> ExecuteAsync(Message message, CancellationToken token)
    {
        try
        {
            var targetUserIds = new List<long>();

            if (message.ReplyToMessage != null)
            {
                targetUserIds.Add(message.ReplyToMessage.From.Id);
            }
            else if (message.Entities != null)
            {
                foreach (var entity in message.Entities.Where(e => e.Type == MessageEntityType.Mention))
                {
                    var username = message.Text.Substring(entity.Offset + 1, entity.Length - 1);
                    var userId = await userRepository.GetUserIdByUsernameAsync(username, message.Chat.Id, token);
                    if (userId.HasValue)
                    {
                        targetUserIds.Add(userId.Value);
                    }
                }
            }

            if (targetUserIds.Count == 0)
            {
                targetUserIds.Add(message.From.Id);
            }

            var responseMessages = new List<string>();
            foreach (var userId in targetUserIds)
            {
                var (complaints, userName) = await userComplaintService.GetUserComplaintsAsync(userId, message.Chat.Id, token);
                var responseMessage = $"Список жалоб для пользователя {userName}:\n{complaints}";
                responseMessages.Add(responseMessage);
            }
            
            return new CommandResult(message.Chat.Id, responseMessages, message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in ComplaintsListCommand");
            return null;
        }
    }
}